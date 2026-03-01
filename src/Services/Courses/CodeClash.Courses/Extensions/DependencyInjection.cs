using CodeClash.Courses.Domains.Courses;
using CodeClash.MongoDB.Extensions;

namespace CodeClash.Courses.Extensions;

public static class DependencyInjection
{
    private const string DatabaseName = "courses-db";
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddDependencyInjection()
        {
            builder.Services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
            });

            builder.Services.AddValidation();
            return builder;
         }

        public IHostApplicationBuilder AddMongoDb()
        {
            builder.AddMongoDb(DatabaseName);
            var services = builder.Services;

            services.AddMongoCollection<Course>("courses");
            return builder;
        }
     }
 }
