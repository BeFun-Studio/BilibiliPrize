using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

/* I developed this program for some reason.
 * I changed my environment from Windows to Ubuntu so I can't use made tools.
 * Even if I can, I won't use made tools because it's an excellent chance to challenge myself.
 * And developing is *so much fun*.
 * Isoheptane
 * 2020/04/01 15:09 */

namespace BilibiliPrize
{
    public static class BilibiliAPI
    {
        public static string videoInformation = "https://api.bilibili.com/x/web-interface/view?aid={0}";
        public static string videoComments = "https://api.bilibili.com/x/v2/reply?jsonp=jsonp&;pn={1}&type=1&oid={0}";
    }
    public class User
    {
        public int uid;
        public string name;
        public int level;
        public List<String> comments;
        public User(int uid,string name,int level, string comment)
        {
            this.uid = uid;
            this.name = name;
            this.level = level;
            comments = new List<string>();
            AddComments(comment);
        }
        public void AddComments(string comment)
        {
            comments.Add(comment);
        }
        public override string ToString() // Serialize Comments
        {
            string ret = string.Format("{0} [UID:{1},Level:{2}]", name, uid, level);
            for(int i = 0;i<comments.Count;i++)
            {
                ret += string.Format("\n{0}:", (i + 1).ToString().PadLeft(4));
                string[] commentLines = comments[i].Replace("\\n", "\n").Split('\n');
                ret += "\"" + commentLines[0];
                for(int j = 1;j<commentLines.Length;j++)
                    ret += "\n      " + commentLines[j];
            }
            ret += "\"\n";
            return ret;
        }
    }
    class MainClass
    {
        /// <summary>
        /// Get response from website via HTTP
        /// </summary>
        /// <returns>What the website response.</returns>
        /// <param name="url">Website URL</param>
        public static string HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/plain;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseString = reader.ReadToEnd();
            reader.Close();
            responseStream.Close();
            return responseString;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Bilibili Draw v0.1.0 by Isoheptane\n");

            string aid;
            VideoInfo videoInfo;

            // Check Video Information
            while (true)
            {
                Console.Write("Bilibili video AID: ");
                aid = Console.ReadLine();
                // Get video information
                string videoInfoJson = HttpGet(string.Format(BilibiliAPI.videoInformation, aid));
                videoInfo = JsonConvert.DeserializeObject<VideoInfo>(videoInfoJson);
                if (videoInfo.code != 0)
                    Console.WriteLine("Video information is invalid , code {0} : {1}", videoInfo.code, videoInfo.message); // Check video available
                else
                    break;
            }
            // Display Video Information
            Console.WriteLine("\nVideo information:");
            Console.WriteLine("  Title: {0}", videoInfo.data.title);
            Console.WriteLine("  AID/BVID: {0} / {1}", videoInfo.data.aid, videoInfo.data.bvid);
            Console.WriteLine("  Author: {0}", videoInfo.data.owner.name);
            Console.WriteLine("  Author UID: {0}", videoInfo.data.owner.mid);
            Console.WriteLine("  Description: ");
            foreach (string line in videoInfo.data.desc.Replace("\\n", "\n").Split('\n'))
                Console.WriteLine("    {0}", line);
            Console.WriteLine("  View: {0}  Danmaku: {1}  Comments: {2}", videoInfo.data.stat.view.ToString().PadLeft(8)
                                                                        , videoInfo.data.stat.danmaku.ToString().PadLeft(5)
                                                                        , videoInfo.data.stat.reply.ToString().PadLeft(7));
            Console.WriteLine("  Like: {0}  Coin: {1}  Favorite: {2}", videoInfo.data.stat.like.ToString().PadLeft(8)
                                                                        , videoInfo.data.stat.coin.ToString().PadLeft(8)
                                                                        , videoInfo.data.stat.favorite.ToString().PadLeft(7));
            Console.WriteLine("  Disike: {0}  Share: {1}", videoInfo.data.stat.dislike.ToString().PadLeft(6)
                                                         , videoInfo.data.stat.share.ToString().PadLeft(7));
            Console.Write("\nPress Enter to check comments.");
            Console.ReadLine();

            // Check Comments
            Console.WriteLine("Getting comment informations...");
            List<User> users = new List<User>();
            int page = 1;
            while(true)
            {
                string commentsJson = HttpGet(string.Format(BilibiliAPI.videoComments, aid, page));
                CommentsInfo commentsInfo = JsonConvert.DeserializeObject<CommentsInfo>(commentsJson);
                CommentsData data = commentsInfo.data;
                if (data.replies == null)
                    break;
                // Analyze Comments
                foreach (CommentsReply replyObject in data.replies)
                {
                    bool exist = false;
                    foreach (User user in users)
                        if (user.uid == replyObject.mid) // Check is user recorded
                        {
                            exist = true;
                            user.AddComments(replyObject.content.message);
                            break;
                        }
                    if (!exist)
                        users.Add(new User(replyObject.mid, replyObject.member.uname, replyObject.member.level_info.current_level, replyObject.content.message));
                    if (replyObject.replies != null) // Second layer searching, same as top layer
                    {
                        foreach (CommentsReply replyReplyObject in replyObject.replies)
                        {
                            exist = false;
                            foreach (User user in users)
                                if (user.uid == replyReplyObject.mid)
                                {
                                    exist = true;
                                    user.AddComments(replyReplyObject.content.message);
                                    break;
                                }
                            if (!exist)
                                users.Add(new User(replyReplyObject.mid, replyReplyObject.member.uname, replyReplyObject.member.level_info.current_level, replyReplyObject.content.message));
                        }
                    }
                }
                page++;
            }
            page--;
            Console.Write("\n{0} users replied, press Enter to display.", users.Count);
            Console.ReadLine();
            // Display all users and comments
            foreach(User user in users)
            {
                Console.WriteLine("  " + user.ToString());
            }
            // Exceptions
            while (true)
            {
                Console.Write("Would you like to except yourself from prize list? <y/n> ");
                string input = Console.ReadLine();
                if(input=="y")
                {
                    Console.Write("Your UID: ");
                    int uid = int.Parse(Console.ReadLine());
                    foreach(User user in users)
                        if(user.uid==uid)
                        {
                            users.Remove(user);
                            break;
                        }
                    break;
                }
                else if(input=="n")
                {
                    break;
                }
            }
            Console.Write("\n{0} user is in the list.How many users would you like to give prize : ",users.Count);
            int prizeCount = int.Parse(Console.ReadLine());
            // Randomly select users.
            Random random = new Random();
            List<User> winners = new List<User>();
            int winnerCount = 0;
            while(winnerCount!=prizeCount)
            {
                int target = random.Next(0,users.Count); // Random won't generate a number equals to its maxValue
                if (winners.Contains(users[target]))
                    continue;
                else
                {
                    winners.Add(users[target]);
                    winnerCount++;
                }
            }
            Console.WriteLine("\nComplete. Winners are:");
            foreach(User user in winners)
            {
                Console.WriteLine("  " + user.ToString());
            }
            Console.WriteLine("Congratulations!");
        }

        // It is really painful feelings.
        // Making Json Structure
        // Video Information API response Json Object
        public class VideoInfo
        {
            public int code { get; set; }
            public string message { get; set; }
            public VideoInfoData data { get; set; }
        }
        public class VideoInfoData
        {
            public string bvid { get; set; }
            public int aid { get; set; }
            public string title { get; set; }
            public string desc { get; set; }
            public VideoInfoDataOwner owner { get; set; }
            public VideoInfoDataStat stat { get; set; }
        }
        public class VideoInfoDataOwner
        {
            public int mid { get; set; }
            public string name { get; set; }
        }
        public class VideoInfoDataStat
        {
            public int view { get; set; }
            public int danmaku { get; set; }
            public int reply { get; set; }
            public int like { get; set; }
            public int dislike { get; set; }
            public int coin { get; set; }
            public int favorite { get; set; }
            public int share { get; set; }
        }
        public class CommentsInfo
        {
            public CommentsData data { get; set; }
        }
        public class CommentsData
        {
            public List<CommentsReply> replies { get; set; }
        }
        public class CommentsReply
        {
            public int mid { get; set; }
            public CommentsReplyMember member { get; set; }
            public CommentsReplyContent content { get; set; }
            public List<CommentsReply> replies { get; set; }
        }
        public class CommentsReplyMember
        {
            public string uname { get; set; }
            public CommentsReplyMemberLevelinfo level_info { get; set; }
        }
        public class CommentsReplyMemberLevelinfo
        {
            public int current_level { get; set; }
        }
        public class CommentsReplyContent
        {
            public string message { get; set; }
        }

    }
}
