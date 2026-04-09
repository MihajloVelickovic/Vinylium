import { Outlet, Link } from 'react-router-dom';
import "../styles/AdminDashboard.css"

export const AdminDashboard = () => {
    return (
        <div className="admin-wrapper">
            <h2>Welcome, Admin!</h2>
            <div className="admin-sidebar">
                <Link to="/admin/add-album"><button className="admin-link">Add Product</button></Link>
                <Link to="/admin/manage-store"><button className="admin-link">Manage Products</button></Link>
                <Link to="/admin/add-store"><button className="admin-link">Add Store</button></Link>
                <Link to="/admin/manage-users"><button className="admin-link">Manage Users</button></Link>
            </div>
            
            <div className="admin-content">
                <Outlet />
            </div>
        </div>
    );
}