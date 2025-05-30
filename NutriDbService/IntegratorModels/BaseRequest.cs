﻿namespace NutriDbService.IntegratorModels
{
    public abstract class BaseRequest
    {
        public long TgId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public abstract string Status { get;}
    }
}
