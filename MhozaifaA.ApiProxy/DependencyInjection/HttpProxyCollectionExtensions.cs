using MhozaifaA.ApiProxy.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MhozaifaA.ApiProxy.DependencyInjection
{
    public static class HttpProxyCollectionExtensions
    {
        /// <summary>
        /// inject the main Api host to call taken baseadress from congif
        /// <para> Actting as proxy </para>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpProxy(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
            return services.AddHttpProxy(options =>
               configuration!.GetSection(HttpProxyOptions._SectionName).Bind(options));
        }

        /// <summary>
        /// inject the main Api host to call
        /// <para> Actting as proxy </para>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpProxy(this IServiceCollection services, Action<HttpProxyOptions> options)
        {
            services.AddHttpClient<IHttpProxy, HttpProxy>();

            services.Configure(options);
            return services;
        }
    }
}
