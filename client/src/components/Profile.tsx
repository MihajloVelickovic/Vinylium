import {useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import {useAuth} from "./AuthContext.tsx";
import authClient from "../api/AuthClient.ts";
import Order from "../models/Order.ts";
import "../styles/Profile.css"

export const Profile = () => {

    const {logout} = useAuth();
    const [orders, setOrders] = useState<Order[]>([]);
    const [error, setError] = useState<string | null>(null);

    //const params = useParams();

    useEffect(() => {
        authClient.get("/Order/MyOrders")
            .then(res => setOrders(res.data.data.map((raw: any) => new Order(raw))))
            .catch(e => console.error(e));
    }, []);

    const cancelOrder = async (orderId: string) => {
        setError(null);
        await authClient.delete(`/Order/Cancel/${orderId}`)
            .then(() => setOrders(prev => prev.filter(o => o.id !== orderId)))
            .catch(e => setError(e.response?.data?.message ?? e.message));
    }

    return (
        <div className="profile">
            <button className="button-main" onClick={logout}>Logout</button>
            <button className="button-main">Edit</button>

            <div className="my-orders">
                <h2>My Orders</h2>
                {error && <p className="error">{error}</p>}
                {
                    orders.length === 0 ?
                        <p>No orders yet.</p> :
                        orders.map(order => (
                            <div className="order-summary" key={order.id}>
                                <p>{new Date(order.createdAt).toLocaleDateString()}</p>
                                <p>{order.items.length} item(s)</p>
                                <p>{order.total()} RSD</p>
                                {order.canCancel() &&
                                    <button className="button-main" type="button"
                                            onClick={() => cancelOrder(order.id)}>Cancel</button>
                                }
                            </div>
                        ))
                }
            </div>
        </div>
    )
}