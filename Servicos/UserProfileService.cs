using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBot1.Servicos
{
    public class UserProfileService
    {
        private static readonly List<UserProfile> _userProfiles = new List<UserProfile>();

        // Criar um novo perfil de usuário
        public async Task<UserProfile> CreateUserProfileAsync(UserProfile userProfile)
        {
            _userProfiles.Add(userProfile);
            await Task.Delay(100); // Simula um atraso de operação
            return userProfile;
        }

        // Atualizar um perfil de usuário existente
        public async Task<bool> UpdateUserProfileAsync(UserProfile updatedUserProfile)
        {
            var existingProfile = _userProfiles.FirstOrDefault(p => p.UserId == updatedUserProfile.UserId);
            if (existingProfile == null)
            {
                return false; // Perfil não encontrado
            }

            existingProfile.Name = updatedUserProfile.Name;
            existingProfile.Email = updatedUserProfile.Email;
            existingProfile.Address = updatedUserProfile.Address;
            existingProfile.PhoneNumber = updatedUserProfile.PhoneNumber;
          

            await Task.Delay(100); // Simula um atraso de operação
            return true;
        }

        // Consultar um perfil de usuário pelo ID
        public async Task<UserProfile> GetUserProfileByIdAsync(string userId)
        {
            var userProfile = _userProfiles.FirstOrDefault(p => p.UserId == userId);
            await Task.Delay(100); // Simula um atraso de operação
            return userProfile;
        }

        // Consultar todos os perfis de usuários
        public async Task<IEnumerable<UserProfile>> GetAllUserProfilesAsync()
        {
            await Task.Delay(100); // Simula um atraso de operação
            return _userProfiles;
        }
    }
    public class UserProfile
    {
        public string UserId { get; set; } // Identificador único do usuário
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

    }


}
