namespace EternalSolutions.Samples.B2C.Common
{
    public static class Constants
    {
        public const string ScopeClaim = "http://schemas.microsoft.com/identity/claims/scope";

        public const string IdentityProviderClaim = "http://schemas.microsoft.com/identity/claims/identityprovider";

        public static class Scopes
        {
            public static string NotesServiceAppIdUri = "https://eternalsolutionsb2c.onmicrosoft.com/NotesService/";

            public static string NotesServiceReadNotesScope = "ReadNotes";

            public static string NotesServiceWriteNotesScope = "WriteNotes";
        }
    }
}
