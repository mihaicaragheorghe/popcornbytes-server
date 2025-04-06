using System.Text.RegularExpressions;

namespace PopcornBytes.Api.Users;

public partial class UserRegex
{
    [GeneratedRegex(@"^[a-zA-Z0-9._-]+$")]
    public static partial Regex Username();
    
    [GeneratedRegex(@"/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$")]
    public static partial Regex Email();
    
    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,20}$")]
    public static partial Regex Password();
}
