import type Store from "./Store.ts";

export default class StoreQuantityPair {
    store: Store;
    quantity: number;
    constructor(s: Store, q: number) {
        this.store = s;
        this.quantity = q;
    }
}