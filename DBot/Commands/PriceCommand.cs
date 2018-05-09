using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Commands
{
    public class PriceCommand
    {
        static string graviex_ticker = ConfigurationManager.AppSettings["graviex_ticker"];
        static string stockexchange_ticker = ConfigurationManager.AppSettings["stockexchange_ticker"];
        static string stockexchange_url = "https://stocks.exchange/api2/ticker";
        
        [Command("price")]
        [Description("Get current price on GRAVIEX")]        
        public async Task Price(CommandContext ctx)
        {
            StringBuilder builder = new StringBuilder("\r\n");
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                var response = client.GetAsync(graviex_ticker);
                JObject jsonResponse =  JObject.Parse(await response.Result.Content.ReadAsStringAsync());
                JObject ticker = (JObject)jsonResponse["ticker"];

                builder.AppendLine("----- GRAVIEX -----");
                builder.AppendFormat("Price : {0}\r\n", ticker["last"]);
                builder.AppendFormat("Buy : {0}\r\n", ticker["buy"]);
                builder.AppendFormat("Sell : {0}\r\n", ticker["sell"]);
                builder.AppendFormat("Volume : {0}\r\n", ticker["vol"]);
                decimal change = ticker["change"].ToObject<decimal>();
                builder.AppendFormat("Change : {0:P2} ", change);

                var emoji = DiscordEmoji.FromName(ctx.Client, change > 0 ? ":chart_with_upwards_trend:" : ":chart_with_downwards_trend:");
                builder.AppendLine($"{emoji}");
                builder.AppendLine();


                
                response = client.GetAsync(stockexchange_url);
                JArray jsonArray = JArray.Parse(await response.Result.Content.ReadAsStringAsync());
                ticker = (JObject)jsonArray.FirstOrDefault(c => c["market_name"].ToString() == stockexchange_ticker);
                builder.AppendLine("----- STOCKEXCHANGE -----");
                builder.AppendFormat("Price : {0}\r\n", ticker["last"]);
                builder.AppendFormat("Buy : {0}\r\n", ticker["bid"]);
                builder.AppendFormat("Sell : {0}\r\n", ticker["ask"]);
                builder.AppendFormat("Volume : {0}\r\n", ticker["vol"]);
                change = ticker["spread"].ToObject<decimal>();
                builder.AppendFormat("Change : {0:P2} ", change);

                emoji = DiscordEmoji.FromName(ctx.Client, change > 0 ? ":chart_with_upwards_trend:" : ":chart_with_downwards_trend:");
                builder.AppendLine($"{emoji}");
                builder.AppendLine();                

            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.Description = builder.ToString();

            await ctx.RespondAsync("Actual ABS/BTC prices\r\n ", false, embed.Build());
        }
    }
}
