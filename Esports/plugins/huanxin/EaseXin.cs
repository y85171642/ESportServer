
namespace HXComm
{
    using System;
    using System.Text;
    using System.IO;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using SimpleJSON;
    using Esports.space;    /// <summary>
                            /// 环信服务器端会员访问接口Demo
                            /// Author：Mr.Hu
                            /// QQ:346163801
                            /// Email:346163801@qq.com
                            /// 如有任何问题，可QQ或邮箱联系
                            /// </summary>
    public class EaseXin
    {
        string reqUrlFormat = "https://a1.easemob.com/{0}/{1}/";
        public string clientID { get; set; }
        public string clientSecret { get; set; }
        public string appName { get; set; }
        public string orgName { get; set; }
        public string token { get; set; }
        public string easeMobUrl { get { return string.Format(reqUrlFormat, orgName, appName); } }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="easeAppClientID">client_id</param>
        /// <param name="easeAppClientSecret">client_secret</param>
        /// <param name="easeAppName">应用标识之应用名称</param>
        /// <param name="easeAppOrgName">应用标识之登录账号</param>
        public EaseXin(string easeAppClientID, string easeAppClientSecret, string easeAppName, string easeAppOrgName)
        {
            this.clientID = easeAppClientID;
            this.clientSecret = easeAppClientSecret;
            this.appName = easeAppName;
            this.orgName = easeAppOrgName;
            this.token = QueryToken();
        }

        /// <summary>
        /// 使用app的client_id 和 client_secret登陆并获取授权token
        /// </summary>
        /// <returns></returns>
        string QueryToken()
        {
            if (string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(clientSecret)) { return string.Empty; }
            string cacheKey = clientID + clientSecret;
            if (System.Web.HttpRuntime.Cache.Get(cacheKey) != null &&
                System.Web.HttpRuntime.Cache.Get(cacheKey).ToString().Length > 0)
            {
                return System.Web.HttpRuntime.Cache.Get(cacheKey).ToString();
            }

            string postUrl = easeMobUrl + "token";
            StringBuilder _build = new StringBuilder();
            _build.Append("{");
            _build.AppendFormat("\"grant_type\": \"client_credentials\",\"client_id\": \"{0}\",\"client_secret\": \"{1}\"", clientID, clientSecret);
            _build.Append("}");

            string postResultStr = ReqUrl(postUrl, "POST", _build.ToString(), string.Empty);
            string token = string.Empty;
            int expireSeconds = 0;
            try
            {
                JObject jo = JObject.Parse(postResultStr);
                token = jo.GetValue("access_token").ToString();
                int.TryParse(jo.GetValue("expires_in").ToString(), out expireSeconds);
                //设置缓存
                if (!string.IsNullOrEmpty(token) && token.Length > 0 && expireSeconds > 0)
                {
                    System.Web.HttpRuntime.Cache.Insert(cacheKey, token, null, DateTime.Now.AddSeconds(expireSeconds), System.TimeSpan.Zero);
                }
            }
            catch { return postResultStr; }
            return token;
        }



        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="userName">账号</param>
        /// <param name="password">密码</param>
        /// <returns>创建成功的用户JSON</returns>
        public string AccountCreate(string userName, string password, string nickname = null)
        {
            StringBuilder _build = new StringBuilder();
            _build.Append("{");
            if (string.IsNullOrEmpty(nickname))
                _build.AppendFormat("\"username\": \"{0}\",\"password\": \"{1}\"", userName, password);
            else
                _build.AppendFormat("\"username\": \"{0}\",\"password\": \"{1}\",\"nickname\":\"{2}\"", userName, password, nickname);
            _build.Append("}");
            return AccountCreate(_build.ToString());
        }

        /// <summary>
        /// 创建用户(可以批量创建)
        /// </summary>
        /// <param name="postData">创建账号JSON数组--可以一个，也可以多个</param>
        /// <returns>创建成功的用户JSON</returns>
        public string AccountCreate(string postData) { return ReqUrl(easeMobUrl + "users", "POST", postData, token); }

        /// <summary>
        /// 获取指定用户详情
        /// </summary>
        /// <param name="userName">账号</param>
        /// <returns>会员JSON</returns>
        public string AccountGet(string userName) { return ReqUrl(easeMobUrl + "users/" + userName, "GET", string.Empty, token); }

        /// <summary>
        /// 重置用户密码
        /// </summary>
        /// <param name="userName">账号</param>
        /// <param name="newPassword">新密码</param>
        /// <returns>重置结果JSON(如：{ "action" : "set user password",  "timestamp" : 1404802674401,  "duration" : 90})</returns>
        public string AccountResetPwd(string userName, string newPassword) { return ReqUrl(easeMobUrl + "users/" + userName + "/password", "PUT", "{\"newpassword\" : \"" + newPassword + "\"}", token); }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userName">账号</param>
        /// <returns>成功返回会员JSON详细信息，失败直接返回：系统错误信息</returns>
        public string AccountDel(string userName) { return ReqUrl(easeMobUrl + "users/" + userName, "DELETE", string.Empty, token); }

        public string ReqUrl(string reqUrl, string method, string paramData, string token)
        {
            try
            {
                LOG.Out("Xin ==> Request: " + reqUrl + "\nMethod: " + method + "\nParamData: " + paramData);
                HttpWebRequest request = WebRequest.Create(reqUrl) as HttpWebRequest;
                request.Method = method.ToUpperInvariant();

                if (!string.IsNullOrEmpty(token) && token.Length > 1) { request.Headers.Add("Authorization", "Bearer " + token); }
                if (request.Method.ToString() != "GET" && !string.IsNullOrEmpty(paramData) && paramData.Length > 0)
                {
                    request.ContentType = "application/json";
                    byte[] buffer = Encoding.UTF8.GetBytes(paramData);
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }

                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader stream = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string result = stream.ReadToEnd();
                        LOG.Out("Xin ==> Response: " + "\n" + result);
                        return result;
                    }
                }
            }
            catch (Exception ex) { return ex.ToString(); }
        }

        /*
         *  Path: /{org_name}/{app_name}/users/{owner_username}/contacts/users/{friend_username}
            HTTP Method: POST
         */
        public string AccountAddFriend(string from, string to)
        {
            return ReqUrl(easeMobUrl + "users/" + from + "/contacts/users/" + to, "POST", null, token);
        }

        /*
         *  Path: /{org_name}/{app_name}/users/{owner_username}/contacts/users
            HTTP Method: GET
         */
        public string AccountGetFriends(string uuid)
        {
            return ReqUrl(easeMobUrl + "users/" + uuid + "/contacts/users", "GET", null, token);
        }

        /*  Path: /{org_name}/{app_name}/users/{username}
            HTTP Method: PUT
            URL Params: 无
            Request Headers: {“Authorization”:”Bearer ${token}”}
            Request Body: {“nickname” : “${昵称值}”}
         */
        public string ModifyNickName(string uuid, string nickname)
        {
            return ReqUrl(easeMobUrl + "users/" + uuid, "PUT", "{\"nickname\":\"" + nickname + "\"}", token);
        }

        /*
         * {
            "groupname":"testrestgrp12", //群组名称，此属性为必须的
            "desc":"server create group", //群组描述，此属性为必须的
            "public":true, //是否是公开群，此属性为必须的
            "maxusers":300, //群组成员最大数（包括群主），值为数值类型，默认值200，此属性为可选的
            "approval":true, //加入公开群是否需要批准，默认值是false（加入公开群不需要群主批准），此属性为必选的，私有群必须为true
            "owner":"jma1", //群组的管理员，此属性为必须的
            "members":["jma2","jma3"] //群组成员，此属性为可选的，但是如果加了此项，数组元素至少一个（注：群主jma1不需要写入到members里面）
           }
         */

        public string GroupCreate()
        {
            string postData = "{\"groupname\":\"群名字\",\"desc\":\"描述\",\"public\":true,\"approval\":false,\"owner\":\"10000\",\"maxusers\":300}";
            return ReqUrl(easeMobUrl + "chatgroups", "POST", postData, token);
        }


        /*
         * Path: /{org_name}/{app_name}/chatgroups/{group_id}/users/{username}
            HTTP Method: POST
         */
        public string GroupJoin(string groupID, string uuid)
        {
            return ReqUrl(easeMobUrl + "chatgroups/" + groupID + "/users/" + uuid, "POST", null, token);
        }

        /* Path: /{org_name}/{app_name}/chatgroups/{group_id}/users/{username}
         * HTTP Method: DELETE
        */
        public string GroupExit(string groupID, string uuid)
        {
            return ReqUrl(easeMobUrl + "chatgroups/" + groupID + "/users/" + uuid, "DELETE", null, token);
        }

        public string SendUserInvite(string from, string to,string fromUID,string fromNickname)
        {
            JSONClass jc = new JSONClass();
            jc.Add("form", fromUID);
            return SendMsgToUser(from, to, fromNickname + " 邀请你加入新活动！", jc.ToJSON(0));
        }

        public string UserWelcome(string targetUUID)
        {
            string msg = "欢迎来到这里，找到小伙伴一起玩耍吧！";
            return SendMsgToUser(null, targetUUID, msg);
        }

        public string SendMsgToUser(string from, string to, string msg, string extJson = null)
        {
            return SendMessage("users", new string[] { to }, "txt", msg, from, extJson);
        }

        public string SendMsgToGroup(string from, string groupID, string msg, string extJson = null)
        {
            return SendMessage("chatgroups", new string[] { groupID }, "txt", msg, from, extJson);
        }

        public string SendGroupNotice(string groupID, string msg, string extJson = null)
        {
            return SendMsgToGroup(null, groupID, msg, extJson);
        }

        public string SendMessageByAdmin(string targetType, string[] targetID, string msgType, string msgText, string extJson = null)
        {
            return SendMessage(targetType, targetID, msgType, msgText, null, extJson);
        }

        /*
         *{
	            "target_type":"users",     // users 给用户发消息。chatgroups 给群发消息，chatrooms 给聊天室发消息
	            "target":["testb","testc"], // 注意这里需要用数组，数组长度建议不大于20，即使只有  
                                            // 一个用户u1或者群组，也要用数组形式 ['u1']，给用户发  
                                            // 送时数组元素是用户名，给群组发送时数组元素是groupid
	            "msg":{  //消息内容
		            "type":"txt",  // 消息类型，不局限与文本消息。任何消息类型都可以加扩展消息
		            "msg":"消息"    // 随意传入都可以
	            },
	            "from":"testa",  //表示消息发送者。无此字段Server会默认设置为"from":"admin"，有from字段但值为空串("")时请求失败
	            "ext":{   //扩展属性，由APP自己定义。可以没有这个字段，但是如果有，值不能是"ext:null"这种形式，否则出错
		            "attr1":"v1"   // 消息的扩展内容，可以增加字段，扩展消息主要解析不分。
	            }
            } 
         */
        public string SendMessage(string targetType, string[] targetID, string msgType, string msgText, string fromUUID, string extJson = null)
        {
            JSONClass jc = new JSONClass();
            jc.Add("target_type", JD(targetType));
            JSONArray ja = new JSONArray();
            foreach (string tID in targetID)
            {
                ja.Add(JD(tID));
            }
            jc.Add("target", ja);
            JSONClass jmsg = new JSONClass();
            jmsg.Add("type", JD(msgType));
            jmsg.Add("msg", JD(msgText));
            jc.Add("msg", jmsg);
            if (fromUUID != null)
                jc.Add("from", fromUUID);
            if (extJson != null)
                jc.Add("ext", JSON.Parse(extJson));

            string postData = jc.ToJSON(0);
            string result = ReqUrl(easeMobUrl + "messages", "POST", postData, token);
            return result;
        }

        public static JSONData JD(object obj)
        {
            return Esports.space.JsonGen.JD(obj);
        }
    }
}
