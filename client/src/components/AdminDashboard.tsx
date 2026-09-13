import { Outlet, Link } from 'react-router-dom';
import "../styles/AdminDashboard.css"

export const AdminDashboard = () => {
    return (
        <div className="admin-wrapper" data-testid="admin-dashboard">
            <h2>Welcome, Admin!</h2>
            <div className="admin-sidebar">
                <Link to="/admin/add-album"><button className="admin-link" data-testid="admin-link-add-product">Add Product</button></Link>
                <Link to="/admin/manage-products"><button className="admin-link" data-testid="admin-link-manage-products">Manage Products</button></Link>
                <Link to="/admin/add-store"><button className="admin-link" data-testid="admin-link-add-store">Add Store</button></Link>
                <Link to="/admin/manage-stores"><button className="admin-link" data-testid="admin-link-manage-stores">Manage Stores</button></Link>
                <Link to="/admin/manage-users"><button className="admin-link" data-testid="admin-link-manage-users">Manage Users</button></Link>
            </div>
            
            <div className="admin-content">
                <Outlet />
            </div>
        </div>
    );
}