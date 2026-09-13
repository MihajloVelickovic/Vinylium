using app.Enums;
using app.Models;

namespace Vinylium.UnitTests;

public static class TestData{
	public static Product NewProduct(string barcode = "5099969944123", decimal? price = 19.99m, bool inStock = true){
		return new Product{
			Barcode = barcode,
			CatalogNumber = "PCS 7088",
			Name = "Abbey Road",
			Artist = "The Beatles",
			ImageUrl = "https://img.discogs.com/abbey-road.jpg",
			Price = price,
			Type = ProductType.Vinyl,
			Tracklist = [],
			Runtime = "47:23",
			ReleaseDate = "1969-09-26",
			InStock = inStock
		};
	}

	public static Store NewStore(Guid? id = null, string name = "Vinylium Niš", bool isWarehouse = false){
		return new Store{
			Id = id ?? Guid.CreateVersion7(),
			Name = name,
			Address = "Obrenovićeva 10",
			City = "Niš",
			ContactNumber = "0601234567",
			OpeningTime = new TimeOnly(9, 0),
			ClosingTime = new TimeOnly(21, 0),
			IsWarehouse = isWarehouse
		};
	}

	public static StoreStock NewStock(string barcode, Guid storeId, int quantity){
		return new StoreStock{
			StoreId = storeId,
			ProductBarcode = barcode,
			Quantity = quantity
		};
	}

	public static StoreStock NewStockWithStore(string barcode, Store store, int quantity){
		return new StoreStock{
			StoreId = store.Id,
			Store = store,
			ProductBarcode = barcode,
			Quantity = quantity
		};
	}

	public static CartItem NewCartItem(string barcode, Guid storeId, int quantity, Guid cartId = default){
		return new CartItem{
			CartId = cartId,
			StoreId = storeId,
			ProductBarcode = barcode,
			Quantity = quantity
		};
	}

	public static Cart NewCart(Guid id, params CartItem[] items){
		return new Cart{ Id = id, Items = items.ToList() };
	}

	public static Cart NewOwnedCart(Guid id, Guid userId, params CartItem[] items){
		return new Cart{ Id = id, UserId = userId, Items = items.ToList() };
	}

	public static CartItemView NewCartItemView(string barcode, Guid storeId, int quantity, decimal? price = 19.99m){
		return new CartItemView{
			Barcode = barcode,
			Name = "Abbey Road",
			Artist = "The Beatles",
			ImageUrl = "https://img.discogs.com/abbey-road.jpg",
			Price = price,
			Quantity = quantity,
			StoreId = storeId,
			StoreName = "Vinylium Niš"
		};
	}

	public static CartView NewCartView(Guid id, params CartItemView[] items){
		return new CartView{ Id = id, Items = items.ToList() };
	}

	public static OrderItem NewOrderItem(string barcode, Guid storeId, int quantity, decimal unitPrice = 19.99m){
		return new OrderItem{
			ProductBarcode = barcode,
			StoreId = storeId,
			Quantity = quantity,
			UnitPrice = unitPrice
		};
	}

	public static User NewUser(Guid? id = null, string email = "strale@vinylium.rs", string username = "strale",
		string hashedPassword = "", bool admin = false){
		return new User{
			Id = id ?? Guid.CreateVersion7(),
			Email = email,
			Username = username,
			Password = hashedPassword,
			Admin = admin
		};
	}
}
