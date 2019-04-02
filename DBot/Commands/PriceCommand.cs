using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Commands
{
    public class PriceCommand
    {
        static string graviex_api_url = ConfigurationManager.AppSettings["graviex_api_url"];
        static string stockexchange_ticker = ConfigurationManager.AppSettings["stockexchange_ticker"];
        static string stockexchange_api_url = ConfigurationManager.AppSettings["stockexchange_api_url"];
        static string crex_api_url = ConfigurationManager.AppSettings["crex_api_url"];
        static string coinmarketcap_api_url = ConfigurationManager.AppSettings["coinmarketcap_api_url"];

        [Command("price")]
        [Description("Get current price on GRAVIEX")]        
        public async Task Price(CommandContext ctx)
        {

            Dictionary<string, double> btc_fiat;

            // check if cache is ok 
            if (MemoryCache.Default.Contains("btc_fiat"))
                btc_fiat = (Dictionary<string, double>)MemoryCache.Default["btc_fiat"];
            else
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    var response = client.GetAsync(coinmarketcap_api_url);
                    response.Result.EnsureSuccessStatusCode();
                    JObject jsonResponse = JObject.Parse(await response.Result.Content.ReadAsStringAsync());
                    btc_fiat = new Dictionary<string, double>();
                    btc_fiat.Add("USD", jsonResponse["data"]["quotes"]["USD"]["price"].ToObject<double>());
                    btc_fiat.Add("EUR", jsonResponse["data"]["quotes"]["EUR"]["price"].ToObject<double>());

                    MemoryCache.Default.Add("btc_fiat", btc_fiat, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(1) });
                }
            }

            JObject graviex;
            if (MemoryCache.Default.Contains("graviex"))
                graviex = (JObject)MemoryCache.Default["graviex"];
            else
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    var response = client.GetAsync(graviex_api_url);
                    response.Result.EnsureSuccessStatusCode();
                    JObject jsonResponse = JObject.Parse(await response.Result.Content.ReadAsStringAsync());
                    graviex = (JObject)jsonResponse["ticker"];

                    MemoryCache.Default.Add("graviex", graviex, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(1) });
                }
            }


            JObject stock;
            if (MemoryCache.Default.Contains("stock"))
                stock = (JObject)MemoryCache.Default["stock"];
            else
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    var response = client.GetAsync(stockexchange_api_url);
                    JArray jsonArray = JArray.Parse(await response.Result.Content.ReadAsStringAsync());
                    stock = (JObject)jsonArray.FirstOrDefault(c => c["market_name"].ToString() == stockexchange_ticker);

                    MemoryCache.Default.Add("stock", stock, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(1) });
                }
            }


            JObject crex;
            if (MemoryCache.Default.Contains("crex"))
                crex = (JObject)MemoryCache.Default["crex"];
            else
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    var response = client.GetAsync(crex_api_url);
                    JArray jsonArray = JArray.Parse(await response.Result.Content.ReadAsStringAsync());
                    crex = (JObject)jsonArray.FirstOrDefault();

                    MemoryCache.Default.Add("crex", crex, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(1) });
                }
            }


            StringBuilder builder = new StringBuilder("\r\n");
            
            if (graviex != null)
            {
                builder.AppendLine("----- GRAVIEX -----");
                builder.AppendFormat("Price : {0}\r\n", graviex["last"]);
                builder.AppendFormat("Buy : {0}\r\n", graviex["buy"]);
                builder.AppendFormat("Sell : {0}\r\n", graviex["sell"]);
                builder.AppendFormat("Volume : {0}\r\n", graviex["vol"]);
                decimal change = graviex["change"].ToObject<decimal>();
                builder.AppendFormat("Change : {0:P2} ", change);

                var emoji = DiscordEmoji.FromName(ctx.Client, change > 0 ? ":chart_with_upwards_trend:" : ":chart_with_downwards_trend:");
                builder.AppendLine($"{emoji}");
                builder.AppendLine();
            }


            if(stock != null)
            {
                builder.AppendLine("----- STOCKEXCHANGE -----");
                builder.AppendFormat("Price : {0}\r\n", stock["last"]);
                builder.AppendFormat("Buy : {0}\r\n", stock["bid"]);
                builder.AppendFormat("Sell : {0}\r\n", stock["ask"]);
                builder.AppendFormat("Volume : {0}\r\n", stock["vol"]);
                decimal change = stock["spread"].ToObject<decimal>();
                builder.AppendFormat("Change : {0:P2} ", change);

                var emoji = DiscordEmoji.FromName(ctx.Client, change > 0 ? ":chart_with_upwards_trend:" : ":chart_with_downwards_trend:");
                builder.AppendLine($"{emoji}");
                builder.AppendLine();
            }

            if(crex != null)
            {
                builder.AppendLine("----- CREX -----");
                builder.AppendFormat("Price : {0:N8}\r\n", crex["last"]);
                builder.AppendFormat("Buy : {0:N8}\r\n", crex["bid"]);
                builder.AppendFormat("Sell : {0:N8}\r\n", crex["ask"]);
                builder.AppendFormat("Volume : {0:N8}\r\n", crex["volumeInBtc"]);
                decimal change = crex["percentChange"].ToObject<decimal>();
                builder.AppendFormat("Change : {0:P2} ", change);

                var emoji = DiscordEmoji.FromName(ctx.Client, change > 0 ? ":chart_with_upwards_trend:" : ":chart_with_downwards_trend:");
                builder.AppendLine($"{emoji}");
                builder.AppendLine();
            }

            if(btc_fiat != null)
            {
                double avg = Math.Max(graviex["last"].ToObject<double>(), stock["last"].ToObject<double>());
                foreach(var kvp in btc_fiat)
                {
                    builder.AppendLine($"----- {kvp.Key} -----");
                    builder.AppendFormat("BTC Price : {0:N2}\r\n", kvp.Value);
                    builder.AppendFormat("ABS Price : {0:N2}\r\n", avg * kvp.Value);
                    builder.AppendLine();
                }
                                
            }

           
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.Description = builder.ToString();

            await ctx.RespondAsync("Actual ABS prices\r\n ", false, embed.Build());
        }
    }
}
