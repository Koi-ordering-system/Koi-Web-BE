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
        Color[] colors = ColorGenerator.Generate(kois);
        CartItem[] cartItems = CartItemGenerator.Generate(carts, farmKois, kois, colors);
        Order[] orders = OrderGenerator.Generate(users, farms);
        KoiImage[] koiImages = KoiImageGenerator.Generate(kois);
        FarmImage[] farmImages = FarmImageGenerator.Generate(farms);
        Review[] reviews = ReviewGenerator.Generate(users, farms);
        OrderKoi[] orderKois = OrderKoiGenerator.Generate(orders, kois, colors);
        OrderTrip[] orderTrips = OrderTripGenerator.Generate(orders);
        IList<Task> tasks = [];

        tasks.Add(context.Users.AddRangeAsync(users));
        tasks.Add(context.Carts.AddRangeAsync(carts));
        tasks.Add(context.Species.AddRangeAsync(species));
        tasks.Add(context.Kois.AddRangeAsync(kois));
        tasks.Add(context.Farms.AddRangeAsync(farms));
        tasks.Add(context.FarmKois.AddRangeAsync(farmKois));
        tasks.Add(context.CartItems.AddRangeAsync(cartItems));
        tasks.Add(context.Orders.AddRangeAsync(orders));
        tasks.Add(context.Colors.AddRangeAsync(colors));
        tasks.Add(context.KoiImages.AddRangeAsync(koiImages));
        tasks.Add(context.FarmImages.AddRangeAsync(farmImages));
        tasks.Add(context.Reviews.AddRangeAsync(reviews));
        tasks.Add(context.OrderKois.AddRangeAsync(orderKois));
        tasks.Add(context.OrderTrips.AddRangeAsync(orderTrips));
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