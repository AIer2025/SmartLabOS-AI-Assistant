import os
import datetime

# --- 配置区域 ---
# 目标目录路径
target_dir = r"C:\TestClaude\SmartLabOS-AI-Assistant\设计资料\01-SmartLabOS-Card-Templates(知识卡片体系)"
# 输出文件名
output_file = "out.txt"

def get_file_size(size):
    """将字节转换为可读格式"""
    for unit in ['B', 'KB', 'MB', 'GB']:
        if size < 1024.0:
            return f"{size:3.1f} {unit}"
        size /= 1024.0
    return f"{size:.1f} TB"

def get_directory_tree(path):
    """生成目录树结构字符串"""
    tree_str = ""
    for root, dirs, files in os.walk(path):
        level = root.replace(path, '').count(os.sep)
        indent = ' ' * 4 * level
        tree_str += f'{indent}{os.path.basename(root)}/\n'
        sub_indent = ' ' * 4 * (level + 1)
        for f in files:
            tree_str += f'{sub_indent}{f}\n'
    return tree_str

def get_file_details(path):
    """获取文件详细信息"""
    details_str = ""
    details_str += f"{'文件路径':<60} | {'大小':<12} | {'修改时间'}\n"
    details_str += "-" * 100 + "\n"
    
    total_files = 0
    total_size = 0

    for root, dirs, files in os.walk(path):
        for f in files:
            file_path = os.path.join(root, f)
            try:
                stat_info = os.stat(file_path)
                size = get_file_size(stat_info.st_size)
                mtime = datetime.datetime.fromtimestamp(stat_info.st_mtime).strftime('%Y-%m-%d %H:%M:%S')
                
                # 相对路径显示
                rel_path = os.path.relpath(file_path, os.path.dirname(path))
                details_str += f"{rel_path:<60} | {size:<12} | {mtime}\n"
                
                total_files += 1
                total_size += stat_info.st_size
            except Exception as e:
                details_str += f"{file_path} (读取错误: {e})\n"

    details_str += "-" * 100 + "\n"
    details_str += f"总计文件数: {total_files} | 总大小: {get_file_size(total_size)}\n"
    return details_str

# --- 执行生成 ---
if __name__ == "__main__":
    if not os.path.exists(target_dir):
        print(f"错误：找不到目录 {target_dir}")
    else:
        print(f"正在扫描目录: {target_dir} ...")
        
        with open(output_file, 'w', encoding='utf-8') as f:
            # 1. 写入标题
            f.write(f"目录结构报告：01-SmartLabOS-Card-Templates\n")
            f.write(f"生成时间：{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
            f.write("=" * 100 + "\n\n")
            
            # 2. 写入目录树
            f.write("【一、目录树结构】\n\n")
            f.write(get_directory_tree(target_dir))
            f.write("\n" + "=" * 100 + "\n\n")
            
            # 3. 写入文件细节
            f.write("【二、文件详细列表】\n\n")
            f.write(get_file_details(target_dir))
            
        print(f"✅ 成功！文件已生成：{os.path.abspath(output_file)}")