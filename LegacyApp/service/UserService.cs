using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsValidClientData(firstName, lastName, email)) return false;

            if (!IsValidClientAge(dateOfBirth)) return false;

            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);
            
            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            SetCreditLimit(user, client.Type);

            if (HasLowCreditLimit(user)) return false;

            UserDataAccess.AddUser(user);
            return true;
        }

        private static bool HasLowCreditLimit(User user)
        {
            return user.HasCreditLimit && user.CreditLimit < 500;
        }

        private void SetCreditLimit(User user, String clientType)
        {
            switch (clientType)
            {
                case "VeryImportantClient":
                    user.HasCreditLimit = false;
                    break;
                case "ImportantClient":
                {
                    using var userCreditService = new UserCreditService();
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName);
                    creditLimit *= 2;
                    user.CreditLimit = creditLimit;
                    break;
                }
                default:
                {
                    user.HasCreditLimit = true;
                    using var userCreditService = new UserCreditService();
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName);
                    user.CreditLimit = creditLimit;
                    break;
                }
            }
        }

        private static bool IsValidClientAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            return age >= 21;
        }

        private static bool IsValidClientData(string firstName, string lastName, string email)
        {
            return !string.IsNullOrEmpty(firstName) && 
                   !string.IsNullOrEmpty(lastName) && 
                   email.Contains("@") && 
                   email.Contains(".");
        }
    }
}
