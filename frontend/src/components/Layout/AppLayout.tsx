import { useState } from 'react';
import { Outlet, useNavigate, Link } from 'react-router-dom';
import { Layout, Menu, Button } from 'antd';
import { DashboardOutlined, LogoutOutlined, CommentOutlined, ShoppingOutlined, PlusOutlined, StopOutlined, SettingOutlined } from '@ant-design/icons';
import { useAuthStore } from '../../stores/authStore';

const { Header, Content } = Layout;

export function AppLayout() {
  const navigate = useNavigate();
  const { user, logout } = useAuthStore();
  const [current, setCurrent] = useState('dashboard');

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const menuItems = [
    { key: 'dashboard', icon: <DashboardOutlined />, label: <Link to="/">Dashboard</Link> },
    { key: 'comments', icon: <CommentOutlined />, label: <Link to="/comments">Comment Center</Link> },
    { key: 'quick-order', icon: <PlusOutlined />, label: <Link to="/quick-order">Quick Order</Link> },
    { key: 'orders', icon: <ShoppingOutlined />, label: <Link to="/orders">Đơn hàng</Link> },
    { key: 'blacklists', icon: <StopOutlined />, label: <Link to="/blacklists">Blacklist</Link> },
    { key: 'settings', icon: <SettingOutlined />, label: <Link to="/settings">Cài đặt</Link> },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ display: 'flex', alignItems: 'center', gap: 24 }}>
        <div style={{ color: '#fff', fontWeight: 600 }}>LiveCommerce</div>
        <Menu theme="dark" mode="horizontal" selectedKeys={[current]} items={menuItems} onClick={(e) => setCurrent(e.key)} style={{ flex: 1, minWidth: 0 }} />
        <span style={{ color: '#fff' }}>{user?.shopName} - {user?.displayName || user?.username}</span>
        <Button type="link" icon={<LogoutOutlined />} onClick={handleLogout} style={{ color: '#fff' }}>Thoát</Button>
      </Header>
      <Content style={{ padding: 24 }}>
        <Outlet />
      </Content>
    </Layout>
  );
}
