
# 开始

#### 第一步：调用令牌获取 
先调用此接口进行身份认证，认证成功后返回`access_token`及`secret`，其中`access_token`需要在后续请求的Header头中携带，`secret`则作为数据加密及界面的密钥。
`令牌有效期为两小时`，过期后需要重新获取，重新获取不会导致已获取的`access_token`及`secret`立即失效。
- **接口地址：** /api/auth
- **请求方式：** GET/POST
- **请求参数：**
    | 参数名称 | 参数类型 | 出现要求 | 描述                                   |
    | -------- | -------- | -------- | -------------------------------------- |
    | uid      | string   | R        | 三方系统的用户名                       |
    | pwd      | string   | R        | 三方系统的密码                         |

- **输出参数：**
    | 参数名称 | 参数类型 | 出现要求 | 描述                                  |
    | -------- | -------- | -------- | ------------------------------------- |
    | code     | string   | R        | 执行状态：0-成功，其它表示异常        |
    | message  | string   | R        | 消息：错误消息或成功提示              |
    | data     | object   | R        |                                |
    | - secret       | string   | O  | 后续请求通讯密钥               |
    | - access_token | string   | O  | 请求通讯令牌                   |
    | - expire_in    | int      | O  | 令牌失效哦啊（秒）             |

- **请求示例：**
```
{uid:'test',pwd:'test'}
```
- **返回示例：**
```
{
     "code": "1",
     "success": true,
     "message": "认证成功，令牌有效期7200秒",
     "data": {
          "secret": "4891D84319CF9ABC",
          "access_token": "981533964c6898a9d30dc2095f6ea670e1a0a3ebda7b1e3dbe1080bbcb2c152d8f2ea9bde8eb2fda0ae41e063ca04851cf2e40c9cf77783440aea7f4d0443764",
          "expire_in": 7200
     }
}
```


#### 第二步：接口调用
接口统一使用`POST`进行调用，编码方式为`UTF-8`。

- **公共参数：**
    - `access_token`：第一步中获取，可在请求头或JSON消息中携带。
    - `data`：各接口中的请求参数，序列化后的`JSON`字符串通过`SM4/ECB/PKCS7Padding`加密后的内容，密钥为上一步`secret`字段返回的内容。

- **请求示例：**
```
{
    "access_token": "981533964c6898a9d30dc2095f6ea670e1a0a3ebda7b1e3dbe1080bbcb2c152d8f2ea9bde8eb2fda0ae41e063ca04851cf2e40c9cf77783440aea7f4d0443764",
    "data": "fa50c76d6914bbeb3e9c9428a1c0f64254e95fb8476b95e5ea3d498df8dee720"
}
```

- **返回示例：**
```
{
    "node": "logistic",
    "code": "1",
    "data": "e2883f41b75fc06c62dda5329872a02b",
    "success": true,
    "message": "查询完成，无符合记录"
}
```
示例中data参数的加密密钥为`4891D84319CF9ABC`，可使用 https://lzltool.cn/SM4 在线工具进行测试。

