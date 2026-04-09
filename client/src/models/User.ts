import type Product from "./Product.ts";

export default class User {
    email: string;
    username: string;
    password: string;
    admin: boolean;
    cart: Array<Product>;
    
    constructor(jsonData: any) {
        ({
            email: this.email,
            username: this.username,
            password: this.password,
            admin: this.admin,
            cart: this.cart,
        } = jsonData);
    }
}
