import Store from "../models/Store.ts";
import "../styles/StoreCard.css"
export const StoreCard = ({store}: {store: Store}) => {
    
    return (
        <div className="storeCard">
            <div className="item">
                <p>Store Name: </p>
                <p>{store.name}</p>
            </div>
            <div className="item">
                <p>Store Address: </p>
                <p>{store.address}, {store.city}</p>
            </div>
            <div className="item">
                <p>Contact No.: </p>
                <p>{store.contactNumber}</p>    
            </div>
            <div className="item">
                <p>Open: </p>
                <p>{store.openingHours}</p>
            </div>
            <div className="item">
                <p>Close: </p>
                <p>{store.closingHours}</p>
            </div>
        </div>
    )
    
}