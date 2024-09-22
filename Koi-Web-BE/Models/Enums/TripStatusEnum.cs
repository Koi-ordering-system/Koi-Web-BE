namespace Koi_Web_BE.Models.Enums;

public enum TripStatusEnum
{
    Pending,      // The trip has been requested but not yet confirmed
    Confirmed,    // The trip is confirmed and scheduled
    Preparing,    // The trip is being prepared
    InProgress,   // The trip is currently ongoing
    OnHold,       // The trip is temporarily paused
    Completed,    // The trip has concluded successfully
    Cancelled,    // The trip has been cancelled
    Delayed,      // The trip is delayed
    Failed,       // The trip was unsuccessful
    Refunded      // The trip was cancelled and a refund was issued
}