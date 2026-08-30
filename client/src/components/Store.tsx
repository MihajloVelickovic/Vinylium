import {ProductCard} from "./ProductCard.tsx";
import "../styles/Store.css"
import {Filters} from "./Filters.tsx";
import {ProductFilterFields} from "./ProductFilterFields.tsx";
import client from "../api/Client.ts";
import {useProductFilters} from "../hooks/useProductFilters.ts";

const Store = () => {

    const {products, filters, setFilters, change, setChange, searchRef} = useProductFilters(client);

    return (
        <>
            <Filters searchRef={searchRef} params={{filters, setFilters, change, setChange}}>
                <ProductFilterFields params={{filters, setFilters, change, setChange}}/>
            </Filters>
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
export default Store
