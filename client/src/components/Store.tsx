import axios from "axios";
import {useEffect, useState} from "react";
import Product from "../models/Product.ts";
import {ProductCard} from "./ProductCard.tsx";
import "../styles/Store.css"
import {Filters} from "./Filters.tsx";

export const Store = () => {

    const [products, setProducts] = useState(new Array<Product>());
    const [pages, setPages] = useState(1);
    const [items, setItems] = useState(20);
    const [currentPage, setCurrentPage] = useState(1);
    const [url, setUrl] = useState(`http://localhost:1738/api/Product/GetPage/?page=${currentPage}&items=${items}`);
    
    useEffect(() => {
        const fetchData = async () => {
            let result = await axios.get(url);
            let temp = new Array<Product>();
            setPages(result.data.pages);
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
            <div>
                    {Array.from(Array(pages).keys()).map((_, i) => (i+1)).map((n => {
                        return <button className="button-main">{n}</button>
                    }))
                    }
            </div>
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