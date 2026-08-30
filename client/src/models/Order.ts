export type OrderItem = {
    productBarcode: string;
    storeId: string;
    quantity: number;
    unitPrice: number;
};

export default class Order {
    id: string;
    email: string;
    createdAt: string;
    items: OrderItem[];

    constructor(jsonData: any) {
        ({
            id: this.id,
            email: this.email,
            createdAt: this.createdAt,
            items: this.items
        } = jsonData);
    }

    total() {
        return this.items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);
    }

    canCancel() {
        const hoursSinceOrder = (Date.now() - new Date(this.createdAt).getTime()) / (1000 * 60 * 60);
        return hoursSinceOrder <= 24;
    }
}
