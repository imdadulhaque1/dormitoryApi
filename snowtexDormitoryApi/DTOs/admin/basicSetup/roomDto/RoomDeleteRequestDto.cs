﻿namespace snowtexDormitoryApi.DTOs.admin.basicSetup.roomDto
{
    public class RoomDeleteRequestDto
    {
        public required int inactiveBy { get; set; }
        public DateTime? inactiveTime { get; set; }
    }
}
