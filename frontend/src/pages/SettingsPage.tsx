import { useEffect, useState } from 'react';
import { Card, Tabs, Table, Button, Form, Input, InputNumber, Modal, Select, message } from 'antd';
import { usersApi, rolesApi, channelConnectionsApi, followUpsApi } from '../api/client';
import type { UserListDto, RoleListDto, ChannelConnectionDto, FollowUpListDto } from '../api/client';

export function SettingsPage() {
  const [users, setUsers] = useState<UserListDto[]>([]);
  const [roles, setRoles] = useState<RoleListDto[]>([]);
  const [channels, setChannels] = useState<ChannelConnectionDto[]>([]);
  const [followUps, setFollowUps] = useState<FollowUpListDto[]>([]);
  const [channelModalOpen, setChannelModalOpen] = useState(false);
  const [channelForm] = Form.useForm();
  const [loading, setLoading] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const [u, r, c, f] = await Promise.all([
        usersApi.list(),
        rolesApi.list(),
        channelConnectionsApi.list(),
        followUpsApi.list(false),
      ]);
      if (u.data.success && u.data.data) setUsers(u.data.data);
      if (r.data.success && r.data.data) setRoles(r.data.data);
      if (c.data.success && c.data.data) setChannels(c.data.data);
      if (f.data.success && f.data.data) setFollowUps(f.data.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleAddChannel = async (v: { channelType: number; channelName: string }) => {
    try {
      const { data } = await channelConnectionsApi.create(v);
      if (data.success && data.data) {
        message.success('Đã thêm kênh');
        setChannelModalOpen(false);
        channelForm.resetFields();
        load();
      }
    } catch {
      message.error('Lỗi');
    }
  };

  const handleFollowUpDone = async (id: number) => {
    try {
      const { data } = await followUpsApi.markDone(id);
      if (data.success) { message.success('Đã đánh dấu xong'); load(); }
    } catch {
      message.error('Lỗi');
    }
  };

  return (
    <div>
      <h2>Cài đặt</h2>
      <Tabs
        items={[
          {
            key: 'users',
            label: 'Người dùng',
            children: (
              <Card>
                <Table size="small" rowKey="id" dataSource={users} loading={loading} pagination={false}
                  columns={[
                    { title: 'Username', dataIndex: 'username', key: 'username' },
                    { title: 'Tên hiển thị', dataIndex: 'displayName', key: 'displayName' },
                    { title: 'Vai trò', dataIndex: 'roleCode', key: 'roleCode' },
                    { title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', render: (v: boolean) => (v ? 'Hoạt động' : 'Tắt') },
                  ]} />
              </Card>
            ),
          },
          {
            key: 'roles',
            label: 'Vai trò',
            children: (
              <Card>
                <Table size="small" rowKey="id" dataSource={roles} loading={loading} pagination={false}
                  columns={[
                    { title: 'Mã', dataIndex: 'code', key: 'code' },
                    { title: 'Tên', dataIndex: 'name', key: 'name' },
                    { title: 'Quyền', dataIndex: 'permissionCodes', key: 'permissionCodes', render: (arr: string[]) => arr?.join(', ') },
                  ]} />
              </Card>
            ),
          },
          {
            key: 'channels',
            label: 'Kênh kết nối',
            children: (
              <Card>
                <Button type="primary" size="small" style={{ marginBottom: 12 }} onClick={() => setChannelModalOpen(true)}>Thêm kênh</Button>
                <Table size="small" rowKey="id" dataSource={channels} loading={loading} pagination={false}
                  columns={[
                    { title: 'Loại', dataIndex: 'channelType', key: 'channelType', render: (t: number) => (t === 0 ? 'TikTok' : t === 1 ? 'Facebook' : 'Khác') },
                    { title: 'Tên kênh', dataIndex: 'channelName', key: 'channelName' },
                    { title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', render: (v: boolean) => (v ? 'Bật' : 'Tắt') },
                  ]} />
              </Card>
            ),
          },
          {
            key: 'followups',
            label: 'Follow-up',
            children: (
              <Card>
                <Table size="small" rowKey="id" dataSource={followUps} loading={loading} pagination={false}
                  columns={[
                    { title: 'Thời hạn', dataIndex: 'targetTimeUtc', key: 'targetTimeUtc', render: (v: string) => new Date(v).toLocaleString() },
                    { title: 'Trạng thái', dataIndex: 'status', key: 'status', render: (s: number) => (s === 0 ? 'Chờ' : s === 1 ? 'Đang xử lý' : 'Xong') },
                    { title: 'Người xử lý', dataIndex: 'assignedUserName', key: 'assignedUserName' },
                    { title: 'Ghi chú', dataIndex: 'note', key: 'note', ellipsis: true },
                    { title: '', key: 'act', width: 80, render: (_, r) => r.status !== 2 ? <Button size="small" onClick={() => handleFollowUpDone(r.id)}>Xong</Button> : null },
                  ]} />
              </Card>
            ),
          },
        ]}
      />
      <Modal title="Thêm kênh kết nối" open={channelModalOpen} onCancel={() => setChannelModalOpen(false)} footer={null}>
        <Form form={channelForm} layout="vertical" onFinish={handleAddChannel}>
          <Form.Item name="channelType" label="Loại kênh" rules={[{ required: true }]}>
            <Select options={[{ value: 0, label: 'TikTok' }, { value: 1, label: 'Facebook' }, { value: 2, label: 'Zalo' }, { value: 99, label: 'Khác' }]} />
          </Form.Item>
          <Form.Item name="channelName" label="Tên kênh" rules={[{ required: true }]}>
            <Input placeholder="Tên hiển thị" />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit">Thêm</Button>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
