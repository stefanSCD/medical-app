using Microsoft.AspNetCore.Identity;
using Moq;

namespace MedicalApp.Tests.Mocks;

public static class MockHelpers
{
    public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

        mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>()))
            .ReturnsAsync(IdentityResult.Success);
        mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>()))
            .ReturnsAsync(IdentityResult.Success);
        mgr.Setup(x => x.AddToRoleAsync(It.IsAny<TUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        return mgr;
    }
}