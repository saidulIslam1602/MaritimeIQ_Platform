#!/usr/bin/env python3
"""Remove all emojis from the codebase
"""import re
import os
from pathlib import Path

# Common emojis used in the codebase
EMOJI_PATTERN = re.compile(
 r'|||||||||||||||||'r'||||||||||||||||'r'|||||||||||||||'r'|||||||||||||||'r'|||||||||||||||'r'||||||||||||')

def remove_emojis_from_file(filepath):
 """Remove emojis from a single file"""try:
 with open(filepath, 'r', encoding='utf-8') as f:
 content = f.read()
 
 original_content = content
 # Remove emojis with a space before them
 content = EMOJI_PATTERN.sub('', content)
 
 # Clean up double spaces left by emoji removal
 content = re.sub(r'+', '', content)
 
 # Clean up space at start of quoted strings
 content = re.sub(r'"\s+', '"', content)
 content = re.sub(r"'\s+", "'", content)
 
 if content != original_content:
 with open(filepath, 'w', encoding='utf-8') as f:
 f.write(content)
 return True
 return False
 except Exception as e:
 print(f"Error processing {filepath}: {e}")
 return False

def main():
 """Remove emojis from all source files"""extensions = ['.cs', '.py', '.md', '.json', '.yml', '.yaml', '.sh', '.txt']
 exclude_dirs = {'node_modules', '.git', 'bin', 'obj', '__pycache__', '.vs', '.vscode'}
 
 root_dir = Path('.')
 modified_files = []
 
 for ext in extensions:
 for filepath in root_dir.rglob(f'*{ext}'):
 # Skip excluded directories
 if any(excluded in filepath.parts for excluded in exclude_dirs):
 continue
 
 if remove_emojis_from_file(filepath):
 modified_files.append(str(filepath))
 print(f"Cleaned: {filepath}")
 
 print(f"\nTotal files modified: {len(modified_files)}")
 return len(modified_files)

if __name__ == '__main__':
 main()
