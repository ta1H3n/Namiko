using System.Collections;
using System.Collections.Generic;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Namiko.Handlers.Attributes;

namespace Namiko.Handlers.ComplexParameters;

public class UserListParams
{
    public IUser User1 { get; set; }
    public IUser User2 { get; set; }
    public IUser User3 { get; set; }
    public IUser User4 { get; set; }
    public IUser User5 { get; set; }

    
    [ComplexParameterCtor]
    public UserListParams(
        [Description("Add user to mention")] IUser user1 = null,
        [Description("Add user to mention")] IUser user2 = null,
        [Description("Add user to mention")] IUser user3 = null,
        [Description("Add user to mention")] IUser user4 = null,
        [Description("Add user to mention")] IUser user5 = null
        )
    {
        User1 = user1;
        User2 = user2;
        User3 = user3;
        User4 = user4;
        User5 = user5;
    }

    public IEnumerable<IUser> GetUsers()
    {
        var users = new List<IUser>();
        
        if (User1 != null)
        {
            users.Add(User1);
        }
        if (User2 != null)
        {
            users.Add(User2);
        }
        if (User3 != null)
        {
            users.Add(User3);
        }
        if (User4 != null)
        {
            users.Add(User4);
        }
        if (User5 != null)
        {
            users.Add(User5);
        }

        return users;
    }
}