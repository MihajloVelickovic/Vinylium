import Store from "../models/Store.ts";
import "../styles/ManageStoreCard.css"
import {Link} from "react-router-dom";

/* the api hands back postgres time values as HH:mm:ss, but the seconds are
 * always 00 since the form only ever collects HH:mm, so they're just noise
 * on the card
 */
const trimSeconds = (time: string) => {
    if (!time)
        return "-";
    const parts = time.split(':');
    return parts.length < 2 ? time : `${parts[0]}:${parts[1]}`;
}

export const ManageStoreCard = ({store}: { store: Store }) => {
    return (
        <Link to={`/admin/manage-stores/${store.id}`}
              style={{textDecoration:'none', color: "var(--text)"}}>
                <div className="manage-store-card">
                    <div className="storeIdentity">
                        <h2>{store.name}</h2>
                        <p className="storeCity">{store.city}</p>
                        <p className="storeHours">
                            {trimSeconds(store.openingHours)} - {trimSeconds(store.closingHours)}
                        </p>
                    </div>
                    <div>
                        <div className="storeInfoBox">
                            <p>Address:</p>
                            <p>{store.address}</p>
                        </div>
                        <div className="storeInfoBox">
                            <p>City:</p>
                            <p>{store.city}</p>
                        </div>
                        <div className="storeInfoBox">
                            <p>Contact No.:</p>
                            <p>{store.contactNumber}</p>
                        </div>
                        <div className="storeInfoBox">
                            <p>Opens:</p>
                            <p>{trimSeconds(store.openingHours)}</p>
                        </div>
                        <div className="storeInfoBox">
                            <p>Clossses:</p>
                            <p>{trimSeconds(store.closingHours)}</p>
                        </div>
                        <div className="storeInfoBox">
                            <p>Warehouse?:</p>
                            <p>{store.isWarehouse ? "Yes" : "No"}</p>
                        </div>
                    </div>
                </div>
        </Link>
    )
}
