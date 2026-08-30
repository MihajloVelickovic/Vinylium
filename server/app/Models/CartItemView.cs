namespace app.Models;
public class CartItemView{
	public required string Barcode{ get; init; }
	public required string Name{ get; init; }
	public required string Artist{ get; init; }
	public required string ImageUrl{ get; init; }
	public decimal? Price{ get; init; }
	public required int Quantity{ get; init; }
	public required Guid StoreId{ get; init; }
	public required string StoreName{ get; init; }
}