# Api接口调用标准

## 1. 编码格式 :id=format

- #### 1.1 字符编码
HTTP通讯及报文`HEX`、`BASE64`编码均采用`UTF-8`字符集编码格式。

- #### 1.2 消息体格式 
    内容均采用`JSON`格式，部分平台接口有加密要求，具体加密方式查看对应产品平台。

- #### 1.3 日期及时间格式
    | 类型 | 格式 | 说明 |
    | ---- | ---- | ---- |
    | 日期 |`yyyy-MM-dd`| 如：`2002-01-01` |
    | 时间 | `HH:mm:ss` | 如：`21:07:00`|
    | 时间戳 | `unix` | 如：`1595482014`非特殊说明精确到秒 |

## 2. 公共参数 :id=base_params
- #### 2.1 请求参数
    | 参数名称 | 参数位置 | 参数类型 | 出现要求 | 描述 |
    | -------- | -------- | -------- | -------- | ---- |
    | access_token | header | string |    C     | 认证参数  |
    | traceid      | header | string |    O     | 请求唯一标识  |

- #### 2.2 输出参数
    | 参数名称 | 参数位置 | 参数类型 | 出现要求 | 描述 |
    | -------- | -------- | -------- | -------- | ---- |
    | code     |   body   | string   |    R     | 状态码，可参考[关于状态码](standard/api.md?id=status_code) |
    | message  |   body   | string   |    R     | 消息：错误消息或成功提示 |
    | data     |   body   | array/object/string | R | 输出数据结果 |
## 3. 字段出现要求 :id=requirements

- #### 3.1 字段出现要求说明
    | 符号 | 说明 |
    | ---- | ---- |
    |  R   | 报文中该字段必须出现（Required） |
    |  O   | 报文中该字段可选出现（Optional） |
    |  C   | 报文中该字段在一定条件下出现（Conditional） |
    |  A   | 呈列表出现，个数不限，允许为零（Array） |

## 4. 关于状态码 :id=status_code
- #### 4.1 统一规范
    | 状态码 | 状态 | 说明 |
    | ---- | ---- | ---- |
    |  0  | 未知异常 unkown error | 未确认具体的错误内容 |
    | 200  | 调用成功 result success | 代表调用成功，流程正常 |
    | 201  | 调用成功 result processing | 代表调用成功，流程正在异步处理中 |
    | 11x  | 交互错误 local error | 网络错误，返序列化失败等，由本地SDK识别返回 |
    | 12x  | 远端错误 remote error | 公共参数，验签解密错误等，由网关判定输出 |
    | 3xx  | 服务状态 service state | 下线、停机维护等 |
    | 4xx  | 身份认证 internal error | 需要认证、用户身份无权限等 |
    | 5xx  | 业务异常 business error | 业务异常，流程错误，缺少参数，数据不存在等 |
    | xxxx | 其它输出 custom output | 自定义输出，推荐使用4位及以上的状态码 |

- #### 4.2 常见错误码
    | 态码 | 状态 | 说明 |
    | ---- | ---- | ---- |
    | 110  | 配置错误 client config error | 本地参数缺少或配置有误 |
    | 111  | 通讯异常 client network error | 通讯异常，网络异常或请求错误 |
    | 112  | 参数错误 response parameter error | 通讯参数丢失、格式错误、不符合输出标准等 |
    | 113  | 时间错位 response time warp | 远端输出时间有错位 |
    | 114  | 输出错误 response format error | 远端输出格式不满足本地要求 |
    | 115  | 密钥错误 response verify sign failed | 远端返回内容验签失败 |
    | 116  | 密钥错误 response content decrypt failed | 远端返回内容无法解密 |
    | 117  | 解析错误 response content deserialize failed | 收到远端输出，但反序列化失败 |    
    | 120  | 配置错误 server config error | 远端参数缺少或配置有误 |
    | 121  | 通讯异常 server gateway error | 远端服务器网关异常，后端节点错误 |
    | 122  | 参数错误 request parameter error | 通讯参数丢失、格式错误、不符合请求标准等 |
    | 123  | 时间错位 request time warp | 本地请求时间有错位 |
    | 124  | 请求错误 request format error | 本地请求容格式不满足远端要求 |
    | 125  | 密钥错误 request verify sign failed | 本地请求内容验签失败 |
    | 126  | 密钥错误 request content decrypt failed | 远端接收内容无法解密 |
    | 127  | 解析错误 request content deserialize failed | 远端收到请求，但反序列化失败 |
    | 2xx  | 调用成功 result success | 成功，或进行了部分处理并保存未获取到全部预期输出 |
    | 301  | 服务停用 service transfer | 系统部署地址已转移 |
    | 302  | 服务停用 service maintain | 系统正在维护中 |
    | 304  | 服务停用 service stop | 不再提供响应服务 |
    | 401  | 需要认证 unauthorized | 请求要求用户先进行身份认证 |
    | 403  | 停止访问 unit forbidden | 禁止访问，如业务单元已停用等 |
    | 404  | 未知业务 not found | 无此业务，未知的请求内容 |
    | 405  | 无权访问 not allowed | 禁止访问，如无权访问此业务等 |
    | 500  | 业务错误 business error | 业务错误，无具体原因内部异常 |
    | 501  | 参数错误 parameter error | 业务数据的参数丢失、格式错误、不符合数据要求等 |