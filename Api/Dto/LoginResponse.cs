namespace Api.Dto
{
    public record LoginResponse
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string PublicKey { get; set; }
    }
}
