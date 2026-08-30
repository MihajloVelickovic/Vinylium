export type CartItem = {
    barcode: string;
    name: string;
    artist: string;
    imageUrl: string;
    price: number | null;
    quantity: number;
    storeId: string;
    storeName: string;
};

export default class Cart {
    id: string;
    items: CartItem[];

    constructor(jsonData: any) {
        ({ id: this.id, items: this.items } = jsonData);
    }

    total() {
        return this.items.reduce((sum, i) => sum + (i.price ?? 0) * i.quantity, 0);
    }
}
