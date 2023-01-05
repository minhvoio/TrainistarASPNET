using Microsoft.AspNetCore.Authorization;

namespace TrainistarASPNET.Models
{
    public class Policies
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Trainer = "Trainer";
        public const string Student = "Student";

        public static AuthorizationPolicy NoLimitPolicy()
        {
            return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Admin, Manager, Trainer, Student).Build();
        }

        public static AuthorizationPolicy FromTrainerPolicy()
        {
            return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Admin, Manager, Trainer).Build();
        }

        public static AuthorizationPolicy FromManagerPolicy()
        {
            return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Admin, Manager).Build();
        }

        public static AuthorizationPolicy AdminPolicy()
        {
            return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Admin).Build();
        }
    }
}
