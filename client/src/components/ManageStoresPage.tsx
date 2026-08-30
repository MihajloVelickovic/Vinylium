
import authClient from "../api/AuthClient.ts";
import {ManageStoreCard} from "./ManageStoreCard.tsx";
import {Filters} from "./Filters.tsx";
import {StoreFilterFields} from "./StoreFilterFields.tsx";
import {useStoreFilters} from "../hooks/useStoreFilters.ts";
import "../styles/ManageStoresPage.css"

export const ManageStoresPage = () => {

    const {stores, filters, setFilters, change, setChange, searchRef, loading, error} = useStoreFilters(authClient);

    return (
        <>
            <Filters searchRef={searchRef} params={{filters, setFilters, change, setChange}}>
                <StoreFilterFields params={{filters, setFilters, change, setChange}}/>
            </Filters>
            {
                loading ?
                    <h2 className="manage-stores-message">Loading stores...</h2> :
                error ?
                    <h2 className="manage-stores-message manage-stores-error">{error}</h2> :
                stores.length === 0 ?
                    <h2 className="manage-stores-message">No stores yet.</h2> :
                    <div className="manage-stores">
                        {
                            stores.map(s => {
                                return <ManageStoreCard key={s.id} store={s}/>
                            })
                        }
                    </div>
            }
        </>
    )
}
