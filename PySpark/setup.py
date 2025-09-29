"""
Setup script for MaritimeIQ PySpark jobs
Standard packaging for production deployment
"""

from setuptools import setup, find_packages

setup(
    name="maritime-pyspark-jobs",
    version="1.0.0",
    description="MaritimeIQ Platform PySpark Batch Processing Jobs",
    author="MaritimeIQ Team",
    packages=find_packages(),
    install_requires=[
        "pyspark>=3.5.0",
        "delta-spark>=3.0.0",
        "pandas>=2.1.0",
        "numpy>=1.26.0",
        "pyarrow>=14.0.0",
    ],
    entry_points={
        'console_scripts': [
            'maritime-voyages=batch_processing_voyages:main',
            'maritime-emissions=emission_analytics:main',
        ],
    },
    python_requires='>=3.9',
)