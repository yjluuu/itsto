﻿using Bo.Interface.IBusiness;
using Common.Tool;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Routine.Models.ApiEntityRequest;
using Routine.Models.ApiEntityResponse;
using Routine.Models.EnmuEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSTOAPI.Controllers
{
    public class CustomerController : BaseController
    {
        private readonly ILog _log;
        private readonly ICustomerService customerService;
        private readonly IAppSettingService appSettingService;
        private readonly IChannelService channelService;
        public CustomerController(ICustomerService customerService, IAppSettingService appSettingService, IChannelService channelService)
        {
            this._log = LogManager.GetLogger(typeof(CustomerController));
            this.customerService = customerService;
            this.appSettingService = appSettingService;
            this.channelService = channelService;
        }

        public IActionResult CustomerRegister(RequestCustomerRegister register)
        {
            ApiBaseResponse response = new ApiBaseResponse();
            #region 校验入参
            if (string.IsNullOrEmpty(register.Brand))
            {
                response.GetErrorApiBaseResponse(ApiBaseResponseStatusCodeEnum.NoDetailedInfo, "Brand不能为空");
                return Ok(response);
            }
            if (string.IsNullOrEmpty(register.NickName))
            {
                response.GetErrorApiBaseResponse(ApiBaseResponseStatusCodeEnum.NoDetailedInfo, "NickName不能为空");
                return Ok(response);
            }
            if (string.IsNullOrEmpty(register.HeadImageUrl))
            {
                response.GetErrorApiBaseResponse(ApiBaseResponseStatusCodeEnum.NoDetailedInfo, "HeadImageUrl不能为空");
                return Ok(response);
            }
            if (string.IsNullOrEmpty(register.JsCode))
            {
                response.GetErrorApiBaseResponse(ApiBaseResponseStatusCodeEnum.NoDetailedInfo, "JsCode不能为空");
                return Ok(response);
            }
            #endregion
            var url = appSettingService.GetAppSettingByKey(register.Brand, "WeChatLoginUrl").AppSettingValue;
            var channel = channelService.GetChannelByBrand(register.Brand);
            string code2SessionReturn = HttpHelper.GetHttp(url + $"?appid={channel.AppId}&secret={channel.Secret}&js_code={register.JsCode}&grant_type=authorization_code");
            _log.Info($"{register.JsCode}获取openid返回：{code2SessionReturn}");
            var code2SessionReturnObj = JsonConvert.DeserializeObject<dynamic>(code2SessionReturn);
            if (code2SessionReturnObj["errcode"] == null)
            {
                register.OpenId = code2SessionReturnObj["openid"];
                register.UnionId = code2SessionReturnObj["unionid"];
                register.SessionKey = code2SessionReturnObj["session_key"];
            }
            string userCode = customerService.CustomerRegister(register);
            if (!string.IsNullOrEmpty(userCode))
            {
                response.ReturnObj = new { UserCode = userCode };
                return Ok(response);
            }
            response.GetErrorApiBaseResponse(ApiBaseResponseStatusCodeEnum.SystemError, "注册失败");
            return Ok(response);
        }
    }
}