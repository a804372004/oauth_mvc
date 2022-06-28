namespace oauth_mvc.Models
{
    internal class LineLogin
    {
        public string Access_token { get; set; }
        public int Expires_in { get; set; }
        public string Id_token { get; set; }
        public string Refresh_token { get; set; }
        public string Scope { get; set; }
        public string Token_type { get; set; }
        public string UserID { get; set; }
        public string DisplayName { get; set; }
    }
}