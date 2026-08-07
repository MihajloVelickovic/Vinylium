import type Store from "./Store.ts";

export default class StoreQuantityPair {
    store: Store;
    quantity: string;
    constructor(s: Store, q: string) {
        this.store = s;
        this.quantity = q;
    }
}