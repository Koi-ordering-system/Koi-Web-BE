using Koi_Web_BE.Database.Generators;
using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Database;

public static class ApplicationDbSeeding
{
    public static async Task Seed(this ApplicationDbContext context)
    {
        try
        {
            await TrySeedAsync(context);
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            throw;
        }
    }
    private static async Task TrySeedAsync(ApplicationDbContext context)
    {
        if (IsDataExist(context)) return;
        User[] users = UserGenerator.Generate();
        Cart[] carts = CartGenerator.Generate(users);
        Species[] species = SpeciesGenerator.Generate();
        Koi[] kois = KoiGenerator.Generate(species);
        Farm[] farms = FarmGenerator.Generate();
        FarmKoi[] farmKois = FarmKoiGenerator.Generate(farms, kois);
        CartItem[] cartItems = CartItemGenerator.Generate(carts, farmKois);
        Order[] orders = OrderGenerator.Generate(users, farms);
        Color[] colors = ColorGenerator.Generate(kois);
        KoiImage[] koiImages = KoiImageGenerator.Generate(kois);
        FarmImage[] farmImages = FarmImageGenerator.Generate(farms);
        Review[] reviews = ReviewGenerator.Generate(users, farms);
        OrderKoi[] orderKois = OrderKoiGenerator.Generate(orders, kois);
        OrderTrip[] orderTrips = OrderTripGenerator.Generate(orders);
        IList<Task> tasks = [];

        await context.Users.AddRangeAsync(users);
        await context.Carts.AddRangeAsync(carts);
        await context.Species.AddRangeAsync(species);
        await context.Kois.AddRangeAsync(kois);
        await context.Farms.AddRangeAsync(farms);
        await context.FarmKois.AddRangeAsync(farmKois);
        await context.CartItems.AddRangeAsync(cartItems);
        await context.Orders.AddRangeAsync(orders);
        await context.Colors.AddRangeAsync(colors);
        await context.KoiImages.AddRangeAsync(koiImages);
        await context.FarmImages.AddRangeAsync(farmImages);
        await context.Reviews.AddRangeAsync(reviews);
        await context.OrderKois.AddRangeAsync(orderKois);
        await context.OrderTrips.AddRangeAsync(orderTrips);

        await Task.WhenAll(tasks);

        await context.SaveChangesAsync();
    }
    private static bool IsDataExist(ApplicationDbContext context)
        => context.Carts.Any()
        || context.CartItems.Any()
        || context.Colors.Any()
        || context.Farms.Any()
        || context.FarmImages.Any()
        || context.FarmKois.Any()
        || context.Kois.Any()
        || context.KoiImages.Any()
        || context.Orders.Any()
        || context.OrderKois.Any()
        || context.OrderTrips.Any()
        || context.Reviews.Any()
        || context.Species.Any()
        || context.Users.Any();
}