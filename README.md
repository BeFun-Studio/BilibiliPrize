# BilibiliPrize
Bilibili Prize for video comments.  
这是一个简单的抽奖程序，用来在评论区抽奖。

## 输入值
### Member UID 用户ID
打开自己的个人空间页面,地址栏应该会如下表示:  
`https://space.bilibili.com/<uid>`  
输入`<uid>`部分即可
### Video ID 视频ID
打开视频页面,地址栏应该会如下表示:  
`https://www.bilibili.com/video/<vid>`  
输入`<vid>`部分即可
### SESSDATA cookie
在主站打开任意页面，查看已使用的cookie  
找到`bilibili.com/Cookie/SESSDATA`项  
输入cookie内容即可

# 额外
此程序将只会使用公开的bilibili API
使用的API如下
## API
#### 个人信息
文章、视频的总播放与总推荐数量:`https://api.bilibili.com/x/space/upstat?mid=<uid>`  
关注与粉丝数量:`https://api.bilibili.com/x/relation/stat?vmid=<uid>`  
个人信息:`https://api.bilibili.com/x/space/acc/info?mid=<uid>`
#### 视频信息
视频基本信息(AV号):`https://api.bilibili.com/x/web-interface/view?aid=<aid>`  
视频基本信息(BV号):`https://api.bilibili.com/x/web-interface/view?bvid=<bvid>`  
视频评论:`https://api.bilibili.com/x/v2/reply?jsonp=jsonp&;pn=<page>&type=1&oid=<aid>`
#### 粉丝关系
某一用户与自己的粉丝关系:`https://api.bilibili.com/x/space/acc/relation?mid=<uid>`  
*此功能需要使用cookie*
## 警告
此程序会访问`api.bilibili.com`获取信息  
在使用时请不要让获奖人数超出范围内的总人数，否则将会产生死循环，大量访问公共API可能导致暂时被Ban IP
