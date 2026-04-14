import {useAuth} from "./AuthContext.tsx";
import {Navigate} from "react-router-dom";

export const AdminRoute = ({children}) => {
    const {admin} = useAuth();
    return admin ? children : <Navigate to="/"/>;
}