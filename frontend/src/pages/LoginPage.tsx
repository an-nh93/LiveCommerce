import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Form, Input, Button, Card, message } from 'antd';
import { authApi } from '../api/client';
import { useAuthStore } from '../stores/authStore';

export function LoginPage() {
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.setAuth);
  const [loading, setLoading] = useState(false);

  const onFinish = async (v: { shopCode: string; username: string; password: string }) => {
    setLoading(true);
    try {
      const { data } = await authApi.login(v.shopCode, v.username, v.password);
      if (data.success && data.data) {
        setAuth(data.data.user, data.data.accessToken);
        message.success('Đăng nhập thành công');
        navigate('/');
      } else {
        message.error(data.message || 'Đăng nhập thất bại');
      }
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } } };
      message.error(err.response?.data?.message || 'Lỗi kết nối');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#f0f2f5' }}>
      <Card title="LiveCommerce" style={{ width: 400 }}>
        <Form layout="vertical" onFinish={onFinish} initialValues={{ shopCode: 'SHOP01', username: 'admin' }}>
          <Form.Item name="shopCode" label="Mã cửa hàng" rules={[{ required: true }]}>
            <Input placeholder="SHOP01" />
          </Form.Item>
          <Form.Item name="username" label="Tên đăng nhập" rules={[{ required: true }]}>
            <Input placeholder="admin" />
          </Form.Item>
          <Form.Item name="password" label="Mật khẩu" rules={[{ required: true }]}>
            <Input.Password placeholder="admin123" />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" block loading={loading}>
              Đăng nhập
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
}
