import axios from "axios";
import {useEffect, useState} from "react";
import Product from "../models/Product.ts";
import {ProductCard} from "./ProductCard.tsx";
import "../styles/Store.css"
import {Filters} from "./Filters.tsx";

export const Store = () => {

    const [products, setProducts] = useState(new Array<Product>());
    const [url, _] = useState("http://localhost:1738/api/Product/GetRandomProducts");

    useEffect(() => {
        const fetchData = async () => {
            let result = await axios.get(url);
            let temp = new Array<Product>();
            result.data.data.forEach((item: any) => {
                temp.push(new Product(item));
            })
            return temp;
        }

        fetchData().then(res => setProducts(res))
            .catch(err => console.error(err));

    }, [url])

    return (
        <>
            <Filters setProducts={setProducts} />
            <div className="products">
                {
                    products.map((product) => {
                        return <ProductCard product={product}/>
                    })
                }
            </div>
        </>
    )
}