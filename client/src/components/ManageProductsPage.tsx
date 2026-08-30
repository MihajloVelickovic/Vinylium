import authClient from "../api/AuthClient.ts";
import {Filters} from "./Filters.tsx";
import {ProductFilterFields} from "./ProductFilterFields.tsx";
import {ManageProductCard} from "./ManageProductCard.tsx";
import {useProductFilters} from "../hooks/useProductFilters.ts";
import "../styles/ManageProductPage.css"

export const ManageProductsPage = () => {

    const {products, filters, setFilters, change, setChange, searchRef} = useProductFilters(authClient);

    return (
        <>
            <Filters searchRef={searchRef} params={{filters, setFilters, change, setChange}}>
                <ProductFilterFields params={{filters, setFilters, change, setChange}}/>
            </Filters>
            <div className="products">
                {
                    products.map((product) => {
                        return <ManageProductCard product={product}/>
                    })
                }
            </div>

        </>
    )
}
