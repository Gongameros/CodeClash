using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Shared.Constants;
using CodeClash.MongoDB.Extensions;

namespace CodeClash.Courses.Extensions;

public static class DependencyInjection
{
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
            builder.AddMongoDb(MongoDbConstants.DatabaseName);
            var services = builder.Services;

            services.AddMongoCollection<Course>(MongoDbConstants.CoursesCollectionName);
            return builder;
        }
     }
 }
