﻿namespace Plus.HabboHotel.Permissions
{
    internal class Permission
    {
        public Permission(int Id, string Name, string Description)
        {
            this.Id = Id;
            PermissionName = Name;
            this.Description = Description;
        }

        public int Id { get; set; }
        public string PermissionName { get; set; }
        public string Description { get; set; }
    }
}