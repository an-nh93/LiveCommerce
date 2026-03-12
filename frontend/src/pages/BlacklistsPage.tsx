import { useEffect, useState } from 'react';
import { Card, Table, Button, Form, Input, InputNumber, Modal, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { blacklistsApi, type BlacklistDto } from '../api/client';

export function BlacklistsPage() {
  const [list, setList] = useState<BlacklistDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [form] = Form.useForm();
  const [submitting, setSubmitting] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const { data } = await blacklistsApi.list();
      if (data.success && data.data) setList(data.data);
      else setList([]);
    } catch {
      setList([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const onFinish = async (v: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      const { data } = await blacklistsApi.create({
        phone: v.phone as string,
        address: v.address as string,
        name: v.name as string,
        level: (v.level as number) ?? 3,
        reason: v.reason as string,
      });
      if (data.success && data.data) {
        message.success('Đã thêm blacklist');
        setModalOpen(false);
        form.resetFields();
        load();
      } else message.error(data.message || 'Lỗi');
    } catch {
      message.error('Lỗi');
    } finally {
      setSubmitting(false);
    }
  };

  const columns: ColumnsType<BlacklistDto> = [
    { title: 'SĐT', dataIndex: 'phone', key: 'phone', width: 120 },
    { title: 'Tên', dataIndex: 'name', key: 'name', width: 120 },
    { title: 'Địa chỉ', dataIndex: 'address', key: 'address', ellipsis: true },
    { title: 'Mức', dataIndex: 'level', key: 'level', width: 80 },
    { title: 'Lý do', dataIndex: 'reason', key: 'reason' },
  ];

  return (
    <div>
      <h2>Blacklist</h2>
      <Card>
        <Button type="primary" style={{ marginBottom: 16 }} onClick={() => setModalOpen(true)}>Thêm blacklist</Button>
        <Table rowKey="id" columns={columns} dataSource={list} loading={loading} pagination={false} />
      </Card>
      <Modal title="Thêm blacklist" open={modalOpen} onCancel={() => setModalOpen(false)} footer={null}>
        <Form form={form} layout="vertical" onFinish={onFinish}>
          <Form.Item name="phone" label="Số điện thoại">
            <Input placeholder="SĐT" />
          </Form.Item>
          <Form.Item name="name" label="Tên">
            <Input placeholder="Tên" />
          </Form.Item>
          <Form.Item name="address" label="Địa chỉ">
            <Input placeholder="Địa chỉ" />
          </Form.Item>
          <Form.Item name="level" label="Mức độ" initialValue={3}>
            <InputNumber min={1} max={4} />
          </Form.Item>
          <Form.Item name="reason" label="Lý do">
            <Input.TextArea rows={2} placeholder="Lý do blacklist" />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting}>Thêm</Button>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
